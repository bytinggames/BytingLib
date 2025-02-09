
namespace BytingLib
{
    public class OnInputPressed : IUpdate, IUpdateWhenBelowPopup
    {
        private readonly Func<InputBoolState> input;
        private readonly Action action;
        private readonly bool alsoUpdateBelowPopup;

        public OnInputPressed(Func<InputBoolState> input, Action action, bool alsoUpdateBelowPopup = false)
        {
            this.input = input;
            this.action = action;
            this.alsoUpdateBelowPopup = alsoUpdateBelowPopup;
        }

        public void Update()
        {
            if (input().Pressed)
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
