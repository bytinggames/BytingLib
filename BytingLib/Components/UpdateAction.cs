namespace BytingLib
{
    public class UpdateAction : IUpdate
    {
        private readonly Action action;

        public UpdateAction(Action action)
        {
            this.action = action;
        }
        public void Update()
        {
            action.Invoke();
        }
    }
}
