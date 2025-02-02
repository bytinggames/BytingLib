namespace BytingLib
{
    public class Vector2Mouse : Vector2Input
    {
        public override Vector2 GetValue(FullInput input)
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
