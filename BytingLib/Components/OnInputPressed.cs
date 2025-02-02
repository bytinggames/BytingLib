
namespace BytingLib
{
    public class OnInputPressed : IUpdate, IUpdateWhenBelowPopup
    {
        private readonly BoolOutput boolOutput;
        private readonly Action action;
        private readonly bool alsoUpdateBelowPopup;

        public OnInputPressed(BoolOutput boolOutput, Action action, bool alsoUpdateBelowPopup = false)
        {
            this.boolOutput = boolOutput;
            this.action = action;
            this.alsoUpdateBelowPopup = alsoUpdateBelowPopup;
        }

        public void Update()
        {
            if (boolOutput.Pressed)
            {
                action();
            }
        }

        public void UpdateWhenBelowPopup(Scene popup)
        {
            if (alsoUpdateBelowPopup)
            {
                Update();
            }
        }
    }
}
