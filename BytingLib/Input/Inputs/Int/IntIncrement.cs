namespace BytingLib
{
    public class IntIncrement(InputInt input, InputBool increment, InputBool? decrement) : InputIntSimple
    {
        public InputInt input { get; } = input;
        public InputBool increment { get; } = increment;
        public InputBool? decrement { get; }  = decrement;

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            int value = this.input.GetState(state.Updater).Value;
            if (increment.GetState(state.Updater).Pressed)
            {
                value++;
            }
            if (decrement != null && decrement.GetState(state.Updater).Pressed)
            {
                value--;
            }
            return value;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return input;
            yield return increment;
            if (decrement != null)
            {
                yield return decrement;
            }
        }
    }
}
