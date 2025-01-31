namespace BytingLib
{
    public class Vector2Func(Func<Vector2> getValue) : Vector2Input
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override Vector2 GetValue(FullInput input)
        {
            return getValue();
        }

        public override string ToString()
        {
            return " Func ";
        }
    }
}
