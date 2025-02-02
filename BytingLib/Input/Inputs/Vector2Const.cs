namespace BytingLib
{
    public class Vector2Const : Vector2Input
    {
        private readonly Vector2 value;

        public Vector2Const(Vector2 value)
        {
            this.value = value;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override Vector2 CalculateValue(FullInput input)
        {
            return value;
        }
    }
}
