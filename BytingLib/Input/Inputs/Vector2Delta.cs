namespace BytingLib
{
    public class Vector2Delta(InputVector2 child) : InputVector2Simple
    {
        public override IEnumerable<Input> GetChildren()
        {
            yield return child;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return child.GetState(state.Updater).Delta;
        }

        public override string ToString()
        {
            return " Delta " + child;
        }
    }
}
