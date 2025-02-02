namespace BytingLib
{
    public class Vector2Delta(Vector2Input child) : Vector2Input
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return child;
        }

        protected override Vector2 CalculateValue(FullInput input)
        {
            return child.Delta;
        }

        public override string ToString()
        {
            return " Delta " + child;
        }
    }
}
