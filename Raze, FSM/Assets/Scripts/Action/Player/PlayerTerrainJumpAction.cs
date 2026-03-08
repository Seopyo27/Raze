using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using System;

namespace MP1.Actions.Player
{
    public class PlayerTerrainJumpAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;

        public PlayerTerrainJumpAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        private float _JumpTimer;
        private Vector3 _startPos;
        private Vector3 _targetPos;
        public void Enter()
        {
            ResetTerrainJumpActionValues();
            PlayTerrainJumpAnimation();
            PlayTerrainJumpSound();
        }

        public void Update()
        {
            _playerActionManager.UpdateCrouchLayerWeight();

            _JumpTimer += Time.deltaTime;
            

            if(_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if (_JumpTimer >= _player.TerrainJumpTime)
            {
                if (_playerActionManager.CheckAttackActionAndTransition()) return;
                if (_playerActionManager.CheckRollingActionAndTransition()) return;
                if (_playerActionManager.CheckMoveActionAndTransition()) return;
                if (_playerActionManager.CheckIdleActionAndTransition()) return;
            }

            Vector3 TerrainJumpVelocity = CalTerrainJumpVelocity();
            _player.RotateCharacter(TerrainJumpVelocity, _player.RotationSpeed);
            ApplyTerrainJumpVelocity(TerrainJumpVelocity);   
        }

        public void Exit()
        {
            _player.VerticalVelocity = Vector3.zero;
            _player.HorizontalVelocity = Vector3.zero;

        }

        private void ResetTerrainJumpActionValues()
        {
            _JumpTimer = 0;
            _startPos = _player.transform.position;
            _targetPos = _player.PossibleTerrainJumpPosition;
        }

        private void PlayTerrainJumpAnimation()
        {
            _player.Animator.CrossFade("StartJump", 0.1f);
        }

        private Vector3 CalTerrainJumpVelocity()
        {
            float normalizedTime = _JumpTimer / _player.TerrainJumpTime;

            Vector3 nextTargetPos = Vector3.Lerp(_startPos, _targetPos, normalizedTime);
            float extraY = Mathf.Sin(normalizedTime * Mathf.PI) * _player.TerrainJumpHeight;
            nextTargetPos.y += extraY;

            return (nextTargetPos - _player.transform.position) / Time.deltaTime;
        }

        private void ApplyTerrainJumpVelocity(Vector3 velocity)
        {
            _player.VerticalVelocity = new Vector3(0, velocity.y, 0);
            _player.HorizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        }

        private void PlayTerrainJumpSound()
        {
            _player.AudioSource.PlayOneShot(_player.SoundData.JumpSound);
        }
    }
}