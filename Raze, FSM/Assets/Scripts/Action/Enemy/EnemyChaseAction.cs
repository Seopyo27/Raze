using System;
using MP1.Actions.Core;
using MP1.Combat;
using MP1.Control;
using MP1.Control.Enemey;
using MP1.Stats;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyChaseAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        private enum ChaseState
        {
            None,
            Chase,
            Suspicion
        }

        private ChaseState currentChaseState;
        private float ChasingTimer = 0;
        private float suspicionTimer = 0;
        
        public EnemyChaseAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }

        public void Enter()
        {
            InitChaseActionValues();
        }

        public void Update()
        {
            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;
            
            // 적이 시야에서 보이거나, 발자국 소리가 들리거나, 전투의 함성 소리를 듣는다면 최소 추격 시간 타이머 초기화
            if(CheckFrontalVision()  || CheckFootSound() || CheckBattleCrySound()) 
            {
                ChasingTimer = 0;
            }

            //최소 추격 시간만큼 플레이어 쫓기
            if(ChasingTimer <= _enemy.MinimumChasingTime)
            {
                //플레이어 쫓기
                ChasePlayer();

                //플레이어와 대치할 수 있다면 대치상태로 전이
                if(CanConfrontation())
                {
                    _enemyActionManager.TransitionTo(_enemyActionManager.EnemyConfrontationAction);
                }
            }

            //최소 추격 시간 끝나면
            else
            {
                //의심하기
                Suspect();
                
                //의심시간만큼 의심하면
                if(suspicionTimer >= _enemy.SuspicionTime)
                {
                    //정찰상태로 전이
                    _enemyActionManager.TransitionTo(_enemyActionManager.EnemyPatrolAction);
                }
            }
        }

        public void Exit()
        {
            
        }

        private void InitChaseActionValues()
        {
            currentChaseState = ChaseState.None;
            _enemy.SetSpeed(_enemy.ChaseSpeedFraction);
            ChasingTimer = 0;
            suspicionTimer = 0;
        }

        private void EnterChaseState()
        {   
            if(currentChaseState != ChaseState.Chase)
            {
                currentChaseState = ChaseState.Chase;
                _enemy.Animator.CrossFade("Sprint", 0.1f);
            }
        }

        private void ChasePlayer()
        {
            EnterChaseState();
            _enemy.NavMeshAgent.isStopped = false;
            suspicionTimer = 0;
            ChasingTimer += Time.deltaTime;
            _enemy.NavMeshAgent.destination = _enemy.targetPlayer.transform.position;
        }

        private void EnterSuspicionState()
        {
            if(currentChaseState != ChaseState.Suspicion)
            {
                currentChaseState = ChaseState.Suspicion;
                _enemy.Animator.CrossFade("Idle", 0.1f);
            }
        }

        private void Suspect()
        {   
            EnterSuspicionState();
            suspicionTimer += Time.deltaTime;
            _enemy.NavMeshAgent.isStopped = true;
        }

        private bool CheckFrontalVision()
        {
            return _enemy.Vision.CheckFrontalVision(_enemy.transform);
          
        }

        //플레이어가 대치 거리안에 있고 사이에 시야를 막는 장애물이 없다면 true 리턴
        private bool CanConfrontation()
        {
            if (!IsPlayerInConfrontationRange()) return false;
            if (IsPathBlocked()) return false;
            return true;
        }

        private bool IsPlayerInConfrontationRange()
        {
            float distance = Vector3.Distance(_enemy.transform.position, _enemy.targetPlayer.transform.position);
            return distance <= _enemy.ConfrontationRadius;
        }

        private bool IsPathBlocked()
        {
            Vector3 start = _enemy.transform.position + Vector3.up * 1.0f;
            Vector3 end = _enemy.targetPlayer.transform.position + Vector3.up * 1.0f;

            if(Physics.Linecast(start, end, out RaycastHit hit, LayerMask.GetMask("Ground")))
            {
                return true;
            }
            return false;
        }

        private bool CheckFootSound()
        {
            if(_enemy.HasHeardFootSound)
            {
                _enemy.ResetFootSoundState();
                return true;
            }
            return false;
        }

        private bool CheckBattleCrySound()
        {
            if(_enemy.HasHeardBattleCrySound)
            {
                _enemy.ResetBattleCrySoundState();
                return true;
            }
            return false;
        }

    }
}