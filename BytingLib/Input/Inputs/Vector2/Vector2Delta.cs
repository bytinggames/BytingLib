namespace BytingLib
{
    public class Vector2Delta(InputVector2 child) : InputVector2Simple
    {
        public InputVector2 Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return Child.GetState(state.Updater).Delta;
        }

        public override string ToString()
        {
            return " Delta " + Child;
        }
    }
}
