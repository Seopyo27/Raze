namespace MP1.Actions.Core
{
    public abstract class ActionManager
    {
        protected IAction startingAction;
        public IAction currentAction;

        public virtual void Initialize()
        {
            currentAction = startingAction;
            startingAction.Enter();
        }

        public virtual void TransitionTo(IAction nextAction)
        {
            currentAction.Exit();
            currentAction = nextAction;
            nextAction.Enter();
        }

        public virtual void Update()
        {
            if(currentAction != null)
            {
                currentAction.Update();
            }
        }

        public virtual void SetStartingAction(IAction startingAction)
        {
            this.startingAction = startingAction;
        }
    }
}

