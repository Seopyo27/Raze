using System;
using MP1.Actions.Core;
using MP1.Combat;
using MP1.Control;
using MP1.Control.Enemey;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyPatrolAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        private enum PatrolState
        {
            None,
            Move,
            Stay
        }

        private PatrolState currentPatrolState;
        private int currentWaypointIndex = 0;
        private float waypointStayTimer = 0;

        public EnemyPatrolAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }

        public void Enter()
        {
            InitEnemyPatrolActionValues();
        }

        public void Update()
        {
            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;
            
            // 순찰
            Patrol();

            // 동료의 전투의 함성을 듣는다면 추격 상태로 전이
            if(CheckBattleCrySound())
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyChaseAction);
            }

            // 시야에 플레이가 보이거나 발자국 소리를 들으면 전투의 함성 상태로 전이
            if(CheckFrontalVision() || CheckFootSound())
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyBattleCryAction);
            }
        }

        public void Exit()
        {
            
        }

        private void InitEnemyPatrolActionValues()
        {
            currentPatrolState = PatrolState.None;
            _enemy.SetSpeed(_enemy.PatrolSpeedFraction);
        }

        private void Patrol()
        {
            if (_enemy.PatrolPath == null) return;

            //웨이 포인트에 도착
            if(AtWaypoint())
            {
                //웨이 포인트에서 일정 시간 대기
                StayWayPoint();

                //일정 시간이 지나면 다음 웨이포인트로 갱신
                if (waypointStayTimer <= _enemy.WaypintStayTime) return;
                SetCurrentWayPointToNext();
            }

            //웨이 포인트로 도착하지 않으면
            else
            {
                //웨이 포인트로 이동
                MoveNextPostion();
            }
        }

        private void SetCurrentWayPointToNext()
        {
            waypointStayTimer = 0;
            currentWaypointIndex = _enemy.PatrolPath.GetNextIndex(currentWaypointIndex);
        }

        private void EnterStayState()
        {
            if(currentPatrolState != PatrolState.Stay)
            {
                currentPatrolState = PatrolState.Stay;
                _enemy.Animator.CrossFade("Idle", 0.1f);
            }
        }

        private void StayWayPoint()
        {
            EnterStayState();
            waypointStayTimer += Time.deltaTime;
            _enemy.NavMeshAgent.isStopped = true;
            _enemy.NavMeshAgent.velocity = Vector3.zero;
        }

        private void EnterMoveState()
        {
            if(currentPatrolState != PatrolState.Move)
            {
                currentPatrolState = PatrolState.Move;
                _enemy.Animator.CrossFade("Sprint", 0.1f);
            }  
        }

        private void MoveNextPostion()
        {
            EnterMoveState();
            _enemy.NavMeshAgent.isStopped = false;
            Vector3 nextPosition = _enemy.PatrolPath.GetWayPoint(currentWaypointIndex);
            _enemy.NavMeshAgent.destination = nextPosition;
        }

        private bool AtWaypoint()
        {
            float distacneToWaypoint = Vector3.Distance(_enemy.transform.position, _enemy.PatrolPath.GetWayPoint(currentWaypointIndex));
            return distacneToWaypoint < _enemy.WaypointTolerance;
        }

        private bool CheckFrontalVision()
        {
            return _enemy.Vision.CheckFrontalVision(_enemy.transform);
          
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