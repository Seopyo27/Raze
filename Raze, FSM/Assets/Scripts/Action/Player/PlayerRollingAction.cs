using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using System;

namespace MP1.Actions.Player
{
    public class PlayerRollingAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;
        public PlayerRollingAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        private const float ORIGINAL_ROLL_CLIP_LENGTH = 1.2f;
        private const float ROLL_MOVE_THRESHOLD = 0.62f;
        private float _RollingMoveTimer;  
        private float _RollingMoveTime;
        private float _invincibleStartTime;
        private float _invincibleEndTime;


        public  void Enter()
        {
            ResetRollingActionValues();
            UseStaminaForRolling();

            Vector3 RollingVelocity = CalRollingVelocity();
            ApplyRollingVelocity(RollingVelocity);
            _player.RotateCharacter(RollingVelocity, 100f);

            PlayRollingAnimation();
        }

        public void Update()
        {
            _playerActionManager.UpdateCrouchLayerWeight();
            UpdatePlayerInvincibleValue();

            _RollingMoveTimer += Time.deltaTime;
            if (_RollingMoveTimer >= _RollingMoveTime)
            {
                _player.HorizontalVelocity = Vector3.zero;
            }
            
            if(_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if (_RollingMoveTimer < _player.RollingTime) return;
            if (_playerActionManager.CheckAttackActionAndTransition()) return;
            if (_playerActionManager.CheckMoveActionAndTransition()) return;
            if (_playerActionManager.CheckIdleActionAndTransition()) return;
        }

        public void Exit()
        {
           _player.HorizontalVelocity = Vector3.zero;
           _player.Input.Rolling = false;
        }

        private void UpdatePlayerInvincibleValue()
        {
            _player.IsInvincible = IsInvincible();
        }

        private bool IsInvincible()
        {
            return _invincibleStartTime <= _RollingMoveTimer && _RollingMoveTimer <= _invincibleEndTime;
        }


        private void ResetRollingActionValues()
        {
            float halfTime = _player.RollingTime / 2f;
            float range = halfTime * _player.InvincibleTimeFraction;
            _invincibleStartTime = halfTime - range;
            _invincibleEndTime = halfTime + range;
            _RollingMoveTimer = 0;
            _RollingMoveTime = _player.RollingTime * ROLL_MOVE_THRESHOLD;
        }

        private void PlayRollingAnimation()
        {
            _player.Animator.SetFloat("rollSpeed", ORIGINAL_ROLL_CLIP_LENGTH / _player.RollingTime);
            _player.Animator.CrossFade("Rolling", 0.1f);
        }

        private Vector3 CalRollingVelocity()
        {
            Vector3 rollingDirection;

            if(_player.Input.Move == Vector2.zero)
            {
                rollingDirection = _player.transform.forward;
            }
            else
            {
                Transform camTransform = _player.MainCamera.transform;
                Vector3 camForward = camTransform.forward;
                Vector3 camRight = camTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                rollingDirection = (camRight.normalized * _player.Input.Move.x + camForward.normalized * _player.Input.Move.y).normalized;
            }

            float rollingSpeed = _player.RollingDistacne / _RollingMoveTime;

            return rollingDirection * rollingSpeed;
        }

        private void UseStaminaForRolling()
        {
            _player.Stamina.UseStamina(_player.RollingStaminaCost);
        }

        private void ApplyRollingVelocity(Vector3 rollingVelocity)
        {
            _player.HorizontalVelocity = rollingVelocity;
        }
    }
}