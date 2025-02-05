
namespace BytingLib
{
    public class OnInputReleased : IUpdate
    {
        private readonly Func<InputBoolState> input;
        private readonly Action action;
        private readonly bool alsoUpdateBelowPopup;

        public OnInputReleased(Func<InputBoolState> input, Action action, bool alsoUpdateBelowPopup = false)
        {
            this.input = input;
            this.action = action;
            this.alsoUpdateBelowPopup = alsoUpdateBelowPopup;
        }

        public void Update()
        {
            if (input().Released)
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
