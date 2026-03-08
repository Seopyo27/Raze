using MP1.Actions.Core;
using MP1.Combat;
using MP1.Control.Enemey;
using UnityEngine;

namespace MP1.Actions.Enemy
{
    public class EnemyConfrontationAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
        private enum ConfrontationState
        {
            None,
            MoveAround
        }

        private ConfrontationState currentConfrontationState = ConfrontationState.None;
        private float _confrontationTimer = 0; // 대치 타이머
        private float _nextConfrontationTime; // 대치 시간
        private float _strafeChangeIntervalTimer = 0; // 대치 좌우 변동 타이머
        private float _nextStrafeChangeTime; // 대치 좌우 변동 간격
        private int _strafeDirection = 1; // 1: 오른쪽, -1: 왼쪽

        public EnemyConfrontationAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }
    

        public void Enter()
        {
            InitConfrontationActionValues();
        }

        public void Update()
        {
            // 불균형 상태가 된다면 불균형 상태로 전이
            if(_enemyActionManager.CheckUnbalanceActionAndTransition()) return;

            // 플레이어 주변에서 대치
            MoveAroundPlayer();
            
            // 대치 범위 밖으로 나가거나 플레이어 사이에 장애물이 있다면 추격 상태로 전이
            if (GetDistanceToPlayer() > _enemy.ConfrontationRadius || IsPathBlocked())
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyChaseAction);
                return;
            }

            // 대치 시간이 끝났다면
            if (_confrontationTimer < _nextConfrontationTime) return;
            
            // 공격 범위 안에 플레이어가 들어올때까지 전진
            if (!IsPlayerInAttackRange())
            {
                _enemy.SetSpeed(_enemy.AttackMoveSpeedFraction);
                _enemy.NavMeshAgent.destination = _enemy.targetPlayer.transform.position;
            }

            // 공격 범위 안에 플레이어가 들어왔다면 공격 상태로 전이
            else
            {
                _enemyActionManager.TransitionTo(_enemyActionManager.EnemyAttackAction);
            }
        }

        public void Exit()
        {
            _enemy.NavMeshAgent.updateRotation = true;
        }

        private float GetDistanceToPlayer()
        {
            return Vector3.Distance(_enemy.transform.position, _enemy.targetPlayer.transform.position);
        }

        private void InitConfrontationActionValues()
        {
            currentConfrontationState = ConfrontationState.None;
            _enemy.SetSpeed(_enemy.ConfrontationSpeedFraction);
            _confrontationTimer = 0;
            _strafeChangeIntervalTimer = 0;
            _nextConfrontationTime = GetRandomConfrontationTime();
            _nextStrafeChangeTime = GetRandomStrafeChangeIntervalTime();
        }

        private void EnterMoveAroundPlayer()
        {
            if(currentConfrontationState != ConfrontationState.MoveAround)
            {
                currentConfrontationState = ConfrontationState.MoveAround;
                _enemy.Animator.CrossFade("StrafeMove", 0.1f);
            }
        }

        private void MoveAroundPlayer()
        {
            EnterMoveAroundPlayer();

            _strafeChangeIntervalTimer += Time.deltaTime;
            _confrontationTimer = Mathf.Min(_confrontationTimer + Time.deltaTime, _nextConfrontationTime);

            _enemy.NavMeshAgent.isStopped = false;
            _enemy.NavMeshAgent.updateRotation = false;
            _enemy.RotateCharacter(_enemy.targetPlayer.transform.position - _enemy.transform.position, _enemy.RotationSpeed);
            SetStrafeMoveBlendValue();

            if (_strafeChangeIntervalTimer > _nextStrafeChangeTime)
            {
                _strafeDirection *= -1;
                _strafeChangeIntervalTimer = 0;
                GetRandomStrafeChangeIntervalTime();
            }

            // 플레이어 주위를 원형으로 도는 벡터 계산 후 이동
            _enemy.NavMeshAgent.destination = GetNextStrafePoint();
        }

        private Vector3 GetNextStrafePoint()
        {
            Vector3 offset = (_enemy.transform.position - _enemy.targetPlayer.transform.position).normalized;
            Vector3 strafePos = Quaternion.AngleAxis(_strafeDirection * 20f, Vector3.up) * offset;
            Vector3 destination = _enemy.targetPlayer.transform.position + (strafePos * _enemy.ConfrontationRadius);
            return destination;
        }

        private void SetStrafeMoveBlendValue()
        {
            Vector3 moveDir = (_enemy.NavMeshAgent.destination - _enemy.transform.position).normalized;
            float dotForward = Vector3.Dot(_enemy.transform.forward, moveDir);
            float dotRight = Vector3.Dot(_enemy.transform.right, moveDir);   

            _enemy.Animator.SetFloat("dirX", dotRight);
            _enemy.Animator.SetFloat("dirY", dotForward);
        }

        private float GetRandomStrafeChangeIntervalTime()
        {
            return Random.Range(_enemy.StrafeChangeIntervalMinimumTime, _enemy.StrafeChangeIntervalMaximumTime+1);
        }

        private float GetRandomConfrontationTime()
        {
            return Random.Range(_enemy.ConfrontationMinimumTIme, _enemy.ConfrontationMaximumTIme+1);
        }

        private bool IsPlayerInAttackRange()
        {
            Collider[] targets = AttackRange.GetEnemiesInBox(_enemy.transform, _enemy.Weapon.attackRangeOffset, _enemy.Weapon.attackRangeSize, LayerMask.GetMask("Player"));
            if(targets.Length == 0) return false;
            return true;
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
    }
}