using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using System;
using MP1.Control.Core;

namespace MP1.Actions.Player
{
    public class PlayerUnbalacneAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;

        public PlayerUnbalacneAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        public void Enter()
        {
            PlayUnBalanceAnimation();
        }

        public void Update()
        {
            _playerActionManager.UpdateCrouchLayerWeight();

            if(_player.Balance.IsUnbalance()) return;
            if(_playerActionManager.CheckIdleActionAndTransition()) return;
            if(_playerActionManager.CheckAttackActionAndTransition()) return;
            if(_playerActionManager.CheckRollingActionAndTransition()) return;
            if(_playerActionManager.CheckMoveActionAndTransition()) return;
            if(_playerActionManager.CheckTerrainJumpActionAndTransition()) return;
        }

        public void Exit()
        {
        
        }

        private void PlayUnBalanceAnimation()
        {
            _player.Animator.CrossFade("UnBalance", 0.1f);
        }


    }
}