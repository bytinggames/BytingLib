
namespace BytingLib
{
    public class OnInputReleased : IUpdate
    {
        private readonly BoolOutput boolOutput;
        private readonly Action action;

        public OnInputReleased(BoolOutput boolOutput, Action action)
        {
            this.boolOutput = boolOutput;
            this.action = action;
        }

        public void Update()
        {
            if (boolOutput.Released)
            {
                action();
            }
        }
    }
}
