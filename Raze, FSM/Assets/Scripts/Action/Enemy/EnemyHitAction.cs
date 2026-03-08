using MP1.Actions.Core;
using MP1.Control;
using MP1.Control.Enemey;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyHitAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        private float _elapsedTime;

        public EnemyHitAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }

        public void Enter()
        {
            InitHitActionValues();
            PlayHitAnimation();
            PlayHitSound();
        }

        public void Update()
        {
            _elapsedTime += Time.deltaTime;
            // 맞은지 0.3초가 지나면
            if(_elapsedTime < 0.3f) return;

            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;
            
            // 때린 플레이어를 향해 몸돌리기
            _enemy.RotateCharacter(_enemy.targetPlayer.transform.position - _enemy.transform.position, 10f);

            // 플레이어를 향해 몸을 다 돌렸다면
            if(!_enemy.IsLookAtDirection(_enemy.targetPlayer.transform.position - _enemy.transform.position)) return;

            // 확률에 따라 다음 행동이 정해짐
            // 60% - 반격, 공격상태로 전이 / 40% - 대치, 대치 상태로 전이
            if(Random.value < 0.6f)
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyAttackAction);
            }
            else
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyConfrontationAction);
            }
            
        }

        public void Exit()
        {
            
        }

        private void InitHitActionValues()
        {
            _elapsedTime = 0;
        }

        private void PlayHitAnimation()
        {
            _enemy.Animator.CrossFade("Hit", 0.1f);
        }

        public void PlayHitSound()
        {
            if (_enemy.soundData != null && _enemy.soundData.hitSounds != null && _enemy.soundData.hitSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, _enemy.soundData.hitSounds.Length);
                _enemy.AudioSource.PlayOneShot(_enemy.soundData.hitSounds[randomIndex]);
            }
        }
    }
}