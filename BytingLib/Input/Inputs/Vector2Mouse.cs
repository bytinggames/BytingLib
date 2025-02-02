namespace BytingLib
{
    public class Vector2Mouse : Vector2Input
    {
        protected override Vector2 CalculateValue(FullInput input)
        {
            return input.MouseState.Position.ToVector2();
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override string ToString()
        {
            return $"MousePos ({Value})";
        }
    }
}
