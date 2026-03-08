using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using System;

namespace MP1.Actions.Player
{
    public class PlayerIdleAction : IAction
    {
       
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;

        public PlayerIdleAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        public void Enter()
        {
            PlayIdleAnimation();
        }

        public void Update()
        {
            _playerActionManager.UpdateCrouchLayerWeight();
            _playerActionManager.SwapFallingAnimation("Idle");
            
            if(_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if(_playerActionManager.CheckAttackActionAndTransition()) return;
            if(_playerActionManager.CheckRollingActionAndTransition()) return;
            if(_playerActionManager.CheckMoveActionAndTransition()) return;
            if(_playerActionManager.CheckTerrainJumpActionAndTransition()) return;
        }

        public void Exit()
        {
        
        }

        private void PlayIdleAnimation()
        {
            _player.Animator.CrossFade("Idle", 0.1f);
        }


    }
}