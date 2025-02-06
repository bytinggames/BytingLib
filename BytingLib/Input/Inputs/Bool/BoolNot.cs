namespace BytingLib
{
    public class BoolNot(InputBool child) : InputBoolSimple
    {
        public InputBool Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return !Child.GetState(state.Updater).Down;
        }

        public override string ToString()
        {
            return " NOT " + Child;
        }
    }
}
