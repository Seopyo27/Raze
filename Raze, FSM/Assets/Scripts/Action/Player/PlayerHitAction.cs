using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;


namespace MP1.Actions.Player
{
    public class PlayerHitAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;

        public PlayerHitAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        private float _elapsedTime;

        public void Enter()
        {
            ResetHitActionValues();
            PlayHitAnimation();
            PlayHitSound();

        }

        public void Update()
        {
            _playerActionManager.UpdateCrouchLayerWeight();

            _elapsedTime += Time.deltaTime;

            
            if(_elapsedTime < 0.3f) return;
            if(_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if(_playerActionManager.CheckIdleActionAndTransition()) return;
            if(_playerActionManager.CheckAttackActionAndTransition()) return;
            if(_playerActionManager.CheckRollingActionAndTransition()) return;
            if(_playerActionManager.CheckMoveActionAndTransition()) return;
            if(_playerActionManager.CheckTerrainJumpActionAndTransition()) return;
        }

        public void Exit()
        {
        
        }

        private void ResetHitActionValues()
        {
            _elapsedTime = 0;
        }

        private void PlayHitAnimation()
        {
            _player.Animator.CrossFade("Hit", 0.1f);
        }

        public void PlayHitSound()
        {
            if (_player.SoundData != null && _player.SoundData.hitSounds != null && _player.SoundData.hitSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, _player.SoundData.hitSounds.Length);
                _player.AudioSource.PlayOneShot(_player.SoundData.hitSounds[randomIndex]);
            }
        }


    }
}