namespace BytingLib
{
    public class IntIncrement : InputIntSimple
    {
        private readonly InputInt input;
        private readonly InputBool increment;
        private readonly InputBool? decrement;

        public IntIncrement(InputInt input, InputBool increment, InputBool? decrement)
        {
            this.input = input;
            this.increment = increment;
            this.decrement = decrement;
        }

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
