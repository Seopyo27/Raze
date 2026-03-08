using System;
using MP1.Actions.Core;
using MP1.Control;
using MP1.Control.Enemey;

namespace MP1.Actions.Enemy
{
    public class EnemyUnbalancedAction : IAction
    {
        private EnemyController _enemy;
        private EnemyActionManager _enemyActionManager;
   
        public EnemyUnbalancedAction(EnemyActionManager enemyActionManager)
        {
            _enemyActionManager = enemyActionManager;
            _enemy = enemyActionManager.Enemy;
        }

        public void Enter()
        {
            InitUnbalanceActionValues();
            PlayUnBalanceAnimation();
        }



        public void Update()
        {
            // 불균형 상태가 끝난다면 대치 상태로 전이
            if(_enemy.Balance.IsUnbalance()) return;
            _enemyActionManager.TransitionTo(_enemyActionManager.EnemyConfrontationAction);
        }

        public void Exit()
        {
        
        }

        private void InitUnbalanceActionValues()
        {
            _enemy.NavMeshAgent.isStopped = true;
        }

        private void PlayUnBalanceAnimation()
        {
            _enemy.Animator.CrossFade("UnBalance", 0.1f);
        }
    }
}