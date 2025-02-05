namespace BytingLib
{
    public class BoolMouseWheel : InputBoolSimple
    {
        IntMouseWheel mouseWheelSource = new();
        private readonly bool? onlyUpOrDown;

        public BoolMouseWheel(bool? onlyUpOrDown = null)
        {
            this.onlyUpOrDown = onlyUpOrDown;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return mouseWheelSource;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            var wheelState = mouseWheelSource.GetState(state.Updater);
            if (wheelState.Value != 0)
            {
                if (onlyUpOrDown == null)
                {
                    return true;
                }
                else if (onlyUpOrDown.Value)
                {
                    return wheelState.Value > 0;
                }
                else
                {
                    return wheelState.Value < 0;
                }
            }
            return false;
        }

        public override string ToString()
        {
            if (onlyUpOrDown == null)
            {
                return "Mouse Wheel";
            }
            else
            {
                return "Mouse Wheel " + (onlyUpOrDown.Value ? "Up" : "Down");
            }
        }
    }
}
