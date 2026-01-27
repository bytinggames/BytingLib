namespace BytingLib
{
    public class Vector2Sum(params InputVector2[] children) : InputVector2Simple
    {
        public InputVector2[] Children { get; } = children;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 sum = Vector2.Zero;
            for (int i = 0; i < Children.Length; i++)
            {
                sum += Children[i].GetState(state.Updater).Value;
            }
            return sum;
        }

        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return $"Sum ({string.Join(" + ", Children.Select(f => f.ToString()))})";
        }
    }
}
