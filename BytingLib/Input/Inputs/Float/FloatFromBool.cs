namespace BytingLib
{
    public class FloatFromBool(InputBool child) : InputFloatSimple
    {
        public InputBool Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override float CalculateValue(FullInput input, InputFloatState state)
        {
            return !Child.GetState(state.Updater).Down ? 1f : 0f;
        }

        public override string ToString()
        {
            return " NOT " + Child;
        }
    }
}
