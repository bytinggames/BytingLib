namespace BytingLib
{
    public class IntIncrement(InputInt input, InputBool increment, InputBool? decrement) : InputIntSimple
    {
        public InputInt Input { get; } = input;
        public InputBool Increment { get; } = increment;
        public InputBool? Decrement { get; }  = decrement;

        protected override int CalculateValue(FullInput input, InputIntState state)
        {
            int value = this.Input.GetState(state.Updater).Value;
            if (Increment.GetState(state.Updater).Pressed)
            {
                value++;
            }
            if (Decrement != null && Decrement.GetState(state.Updater).Pressed)
            {
                value--;
            }
            return value;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Input;
            yield return Increment;
            if (Decrement != null)
            {
                yield return Decrement;
            }
        }
    }
}
