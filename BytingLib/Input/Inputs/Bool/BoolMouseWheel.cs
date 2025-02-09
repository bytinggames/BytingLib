namespace BytingLib
{
    public class BoolMouseWheel : InputBoolSimple
    {
        IntMouseWheel mouseWheelSource = new();
        public bool? OnlyUpOrDown { get; }

        public BoolMouseWheel(bool? onlyUpOrDown = null)
        {
            this.OnlyUpOrDown = onlyUpOrDown;
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
                if (OnlyUpOrDown == null)
                {
                    return true;
                }
                else if (OnlyUpOrDown.Value)
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
            if (OnlyUpOrDown == null)
            {
                return "Mouse Wheel";
            }
            else
            {
                return "Mouse Wheel " + (OnlyUpOrDown.Value ? "Up" : "Down");
            }
        }
    }
}
