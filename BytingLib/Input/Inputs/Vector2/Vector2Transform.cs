namespace BytingLib
{
    public class Vector2Transform(InputVector2 child, Func<Matrix> getTransform) : InputVector2Simple
    {
        public InputVector2 Child { get; } = child;
        public Func<Matrix> GetTransform { get; } = getTransform;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            return Vector2.Transform(Child.GetState(state.Updater).Value, GetTransform());
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        public override string ToString()
        {
            return $"Transform({Child})";
        }

    }
}
