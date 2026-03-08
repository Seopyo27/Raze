using MP1.Actions.Core;
using MP1.Control;
using MP1.Control.Enemey;
using Unity.VisualScripting;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyDieAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        public EnemyDieAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }

        public void Enter()
        {
            PlayDieAnimation();
            PlayDieSound();
            _enemy.Despawn(10f);
        }

        public void Update()
        {
  
        }

        public void Exit()
        {
            
        }

        private void PlayDieSound()
        {
            _enemy.AudioSource.PlayOneShot(_enemy.soundData.deathSound);
        }

        private void PlayDieAnimation()
        {
            _enemy.Animator.CrossFade("Die", 0.1f);
        }
    }
}