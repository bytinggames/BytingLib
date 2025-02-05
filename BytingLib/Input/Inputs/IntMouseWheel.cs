namespace BytingLib
{
    public class IntMouseWheel : InputInt<IntMouseWheelState>
    {
        protected override int CalculateValue(FullInput input, IntMouseWheelState state)
        {
            return state.CalculateValue(input.MouseState.ScrollWheelValue);
        }

        public override IntMouseWheelState CreateState(InputUpdater updater)
        {
            return new IntMouseWheelState(updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }

        public override string ToString()
        {
            return $"MouseWheel";
        }
    }
}
