
namespace BytingLib
{
    public class UpdateTrigger : IUpdate
    {
        private readonly Func<bool> trigger;
        private readonly Action action;

        public UpdateTrigger(Func<bool> trigger, Action action)
        {
            this.trigger = trigger;
            this.action = action;
        }

        public void Update()
        {
            if (trigger())
                action();
        }
    }
}
