namespace BytingLib
{
    public class BoolDelayed(InputBool child, int pressedDelay, int releasedDelay) : InputBoolSimple
    {
        public InputBool Child { get; } = child;
        public int PressedDelay { get; } = pressedDelay;
        public int ReleasedDelay { get; } = releasedDelay;

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            var childOutput = Child.GetState(state.Updater);

            if (childOutput.Down)
            {
                if (childOutput.DownTime >= PressedDelay)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (childOutput.ReleasedTime >= ReleasedDelay)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        public override string ToString()
        {
            return "BoolDelayed " + Child.ToString();
        }
    }
}
