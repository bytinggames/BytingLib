namespace BytingLib
{
    public class Vector2Transform(InputVector2 child, Func<Matrix> getTransform) : InputVector2Simple
    {
        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return Vector2.Transform(child.GetState(state.Updater).Value, getTransform());
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return child;
        }

        public override string ToString()
        {
            return $"Transform({child})";
        }

    }
}
