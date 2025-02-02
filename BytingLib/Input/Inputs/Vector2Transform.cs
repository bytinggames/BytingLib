namespace BytingLib
{
    public class Vector2Transform(Vector2Input child, Func<Matrix> getTransform) : Vector2Input
    {
        public override Vector2 CalculateValue(FullInput input)
        {
            return Vector2.Transform(child.CalculateValue(input), getTransform());
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        public override string ToString()
        {
            return $"Transform({child})";
        }
    }
}
