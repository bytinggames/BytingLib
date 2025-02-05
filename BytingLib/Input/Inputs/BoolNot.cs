namespace BytingLib
{
    public class BoolNot(InputBool child) : InputBoolSimple
    {
        public override IEnumerable<Input> GetChildren()
        {
            yield return child;
        }

        protected override bool CalculateValue(FullInput input, InputBoolState state)
        {
            return !child.GetState(state.Updater).Down;
        }

        public override string ToString()
        {
            return " NOT " + child;
        }
    }
}
