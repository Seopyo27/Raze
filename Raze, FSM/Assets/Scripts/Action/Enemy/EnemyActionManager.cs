using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using MP1.Control.Enemey;

namespace MP1.Actions.Enemy
{
    public class EnemyActionManager : ActionManager
    {
        public EnemyController Enemy { get; private set; }
        public IAction EnemyHitAction { get; private set; }
        public IAction EnemyUnbalancedAction { get; private set; }
        public IAction EnemyChaseAction { get; private set; }
        public IAction EnemyPatrolAction { get; private set; }
        public IAction EnemyAttackAction { get; private set; }
        public IAction EnemyConfrontationAction { get; private set; }
        public IAction EnemyDieAction { get; private set; }
        public IAction EnemyBattleCryAction { get; private set; }
   
        public EnemyActionManager(EnemyController enemyController)
        {
            Enemy = enemyController;

            EnemyHitAction = new EnemyHitAction(this);
            EnemyUnbalancedAction = new EnemyUnbalancedAction(this);
            EnemyChaseAction = new EnemyChaseAction(this);
            EnemyPatrolAction = new EnemyPatrolAction(this);
            EnemyAttackAction = new EnemyAttackAction(this);
            EnemyConfrontationAction = new EnemyConfrontationAction(this);
            EnemyDieAction = new EnemyDieAction(this);
            EnemyBattleCryAction = new EnemyBattleCryAction(this);
        }

        // 애니메이션 이벤트를 수신해서 행동을 전환

        // 피격당하면 피격액션으로 전환
        public void TransitionToEnemyHitAction()
        {
            if(Enemy.Health.IsDead()) return;
            TransitionTo(EnemyHitAction);
        }

        // 죽는다면 죽음액션으로 전환
        public void TransitionToEnemyDieAction()
        {
            TransitionTo(EnemyDieAction);
        }

        public bool CheckUnbalanceActionAndTransition()
        {
            if (Enemy.Balance.IsUnbalance())
            {
                TransitionTo(EnemyUnbalancedAction);
                return true;
            }
            return false;
        }

    }
}
