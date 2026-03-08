using MP1.Actions.Core;
using MP1.Combat;
using MP1.Control;
using MP1.Control.Enemey;
using MP1.Event;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyBattleCryAction : IAction, IAnimationEventReceiver
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        
        private bool _isFinishBattleCry;

        public EnemyBattleCryAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }
   

        public void Enter()
        {
            InitBattleCryActionValues();
            PlayBattleCryAnimation();
            PlayBattleCrySound();
        }

        public void Update()
        {
            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;

            // 전투의 함성이 끝났다면 추격 상태로 전이
            if(!_isFinishBattleCry) return;
            _enemyActionManager.TransitionTo(_enemyActionManager.EnemyChaseAction);
        }

        public void Exit()
        {
            StopBattleCrySound();
        }

        // 애니메이션 이벤트
        public void OnAnimationEvent(string eventName)
        {
            // 전투의 함성 애니메이션 중 손을 다 들었다면 전투의 함성 실행
            if (eventName == "OnBattleCry") NoiseBatteCry(_enemy.BattleCryRadius);
            // 전투의 함성 애니메이션이 끝났다면
            else if (eventName == "FinishBattleCry") FinishBattleCry();
        }

        private void InitBattleCryActionValues()
        {
            _isFinishBattleCry = false;
            _enemy.NavMeshAgent.isStopped = true;
        }

        private void PlayBattleCrySound()
        {
            _enemy.AudioSource.clip = _enemy.soundData.BattleCry;
            _enemy.AudioSource.loop = false;
            _enemy.AudioSource.Play();
        }

        private void StopBattleCrySound()
        {
            _enemy.AudioSource.Stop();
        }

        private void PlayBattleCryAnimation()
        {
            _enemy.Animator.CrossFade("BattleCry", 0.1f);
        }

        // 주변 동료에게 소리를 냄
        private void NoiseBatteCry(float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(_enemy.transform.position, radius, LayerMask.GetMask("Enemy"));

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<IHearable>(out var enemy))
                {
                    enemy.OnHearBattleCrySound();
                }
            }
        }

        public void FinishBattleCry()
        { 
            _isFinishBattleCry = true;
        }
    }
}