using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가
using MP1.Actions.Core;
using MP1.Control;
using System;

namespace MP1.Actions.Player
{
    public class PlayerDieAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;
        
        private float _timer; 
       

        public PlayerDieAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }
 
        public void Enter()
        {
            PlayDieAnimation();
            _player.AudioSource.PlayOneShot(_player.SoundData.deathSound);
            
            _timer = 0f; 
        }

        public void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= 3.0f) 
            {
                ReloadScene();
            }
          
        }

        public void Exit()
        {
        }

        private void PlayDieAnimation()
        {
            _player.Animator.CrossFade("Die", 0.1f);
        }

        private void ReloadScene()
        {
            // 현재 활성화된 씬을 가져와 다시 로드
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}