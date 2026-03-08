using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;

namespace MP1.Actions.Player
{
    public class PlayerActionManager : ActionManager
    {
        public PlayerController Player { get; private set; }
        public IAction PlayerMoveAction { get; private set; }
        public IAction PlayerIdleAction { get; private set; }
        public IAction PlayerRollingAction { get; private set; }
        public IAction PlayerTerrainJumpAction { get; private set; }
        public IAction PlayerAttackAction { get; private set; }
        public IAction PlayerHitAction { get; private set; }
        public IAction PlayerUnbalanceAction { get; private set; }
        public IAction PlayerDieAction { get; private set; }
        protected bool _wasFalling;
        public PlayerActionManager(PlayerController playerController)
        {
            Player = playerController;
            PlayerMoveAction = new PlayerMoveAction(this);
            PlayerIdleAction = new PlayerIdleAction(this);
            PlayerRollingAction = new PlayerRollingAction(this);
            PlayerTerrainJumpAction = new PlayerTerrainJumpAction(this);
            PlayerAttackAction = new PlayerAttackAction(this);
            PlayerHitAction = new PlayerHitAction(this);
            PlayerUnbalanceAction = new PlayerUnbalacneAction(this);
            PlayerDieAction = new PlayerDieAction(this);
        }

        public void TransitionToPlayerHitAction()
        {
            if(Player.Health.IsDead()) return;
            TransitionTo(PlayerHitAction);
        }

        public void TransitionToPlayerDieAction()
        {
            TransitionTo(PlayerDieAction);
        }

        public bool CheckMoveActionAndTransition()
        {
            if (Player.Input.Move != Vector2.zero)
            {
                TransitionTo(PlayerMoveAction);
                return true;
            }
            return false;
        }

        public bool CheckRollingActionAndTransition()
        {
            if (Player.IsGround 
                && Player.Input.Rolling
                && Player.Stamina.CanUseStamina(Player.RollingStaminaCost))
            {
                TransitionTo(PlayerRollingAction);
                return true;
            }
            return false;
        }

        public bool CheckIdleActionAndTransition()
        {
            if (Player.Input.Move == Vector2.zero)
            {
                TransitionTo(PlayerIdleAction);
                return true;
            }
            return false;
        }

        public bool CheckTerrainJumpActionAndTransition()
        {
            if (Player.IsGround && Player.PossibleTerrainJumpPosition != Vector3.zero && Player.Input.Sprint)
            {
                TransitionTo(PlayerTerrainJumpAction);
                return true;
            }
            return false;
        }

        public bool CheckAttackActionAndTransition()
        {
            if (Player.IsGround
                && Player.Input.Attack 
                && Player.Stamina.CanUseStamina(Player.Weapon.staminaCost))
            {
                TransitionTo(PlayerAttackAction);
                return true;
            }
            
            return false;
        }

        public bool CheckUnbalanceActionAndTransition()
        {
            if (Player.Balance.IsUnbalance())
            {
                TransitionTo(PlayerUnbalanceAction);
                return true;
            }
            return false;
        }

        public void SwapFallingAnimation(string currentAction)
        {
            if (Player.IsFalling && !_wasFalling)
            {
                Player.Animator.CrossFade("Falling", 0.1f);
                _wasFalling = true;
            }
            else if (!Player.IsFalling && _wasFalling)
            {
                Player.Animator.CrossFade(currentAction, 0.1f);
                _wasFalling = false;
            }
        }

        public void UpdateCrouchLayerWeight()
        {
            float targetWeight = Player.Input.Crouch ? 0.7f : 0f;
            SetCrouchLayerWeight(targetWeight);
        }

        public void SetCrouchLayerWeight(float targetWeight)
        {
            float crouchLayerWeight = Mathf.MoveTowards(
                Player.Animator.GetLayerWeight(1),
                targetWeight,
                Time.deltaTime * Player.CrouchTransitionSpeed);
            Player.Animator.SetLayerWeight(1, crouchLayerWeight);
        }
    }
}