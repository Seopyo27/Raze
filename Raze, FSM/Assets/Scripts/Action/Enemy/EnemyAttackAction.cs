using System;
using MP1.Actions.Core;
using MP1.Combat;
using MP1.Control;
using MP1.Control.Enemey;
using MP1.Event;
using MP1.Stats;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyAttackAction : IAction, IAnimationEventReceiver
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        private bool _isFinishAttack;

        public EnemyAttackAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }


        public void Enter()
        {
            InitAttackActionValues();
            PlayAttackAnimation();
            PlayAttackSound();
        }

        public void Update()
        {   
            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;
            
            // 공격이 끝났다면 대치상태로 전이
            if(!_isFinishAttack) return;
            _enemyActionManager.TransitionTo(_enemyActionManager.EnemyConfrontationAction);
        }

        public void Exit()
        {
           
        }


        // 애니메이션 이벤트
        public void OnAnimationEvent(string eventName)
        {   
            // 공격 애니메이션이 끝나면
            if (eventName == "FinishAttack") FinishAttack();
            // 공격 적중 애니메이션 실행 시 공격 범위 스캔
            else if (eventName == "Hit") ScanAttackRange();
        }

        private void InitAttackActionValues()
        {
            _isFinishAttack = false;
        }

        private void PlayAttackAnimation()
        {
            _enemy.NavMeshAgent.isStopped = true;
            _enemy.Animator.CrossFade("Attack1", 0.1f);
        }

        private void PlayAttackSound()
        {
            _enemy.AudioSource.PlayOneShot(_enemy.Weapon.attackAudioClips[0]);
        }

        public void FinishAttack()
        { 
            _isFinishAttack = true;
        }

        // 공격 범위 내 플레이어 스캔
        public void ScanAttackRange()
        {
            // 플레이어 스캔
            Collider[] colliders = AttackRange.GetEnemiesInBox(_enemy.transform, _enemy.Weapon.attackRangeOffset, _enemy.Weapon.attackRangeSize, LayerMask.GetMask("Player"));
            foreach(Collider collider in colliders)
            {
                Health enemyHealth = collider.GetComponent<Health>();
                Balance enemyBalance = collider.GetComponent<Balance>();
                
                if(enemyHealth != null)
                {
                    // 채력 대미지 입힘
                    enemyHealth.TakeDamage(_enemy.Weapon.damage);
                }

                if(enemyBalance != null)
                {
                    // 밸런스 대미지 입힘
                    enemyBalance.TakeBalanceDamage(_enemy.Weapon.balanceDamage);
                } 
            }
        }


    }
}