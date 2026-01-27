namespace BytingLib
{
    public class Vector2MultiplyChildren(params InputVector2[] children) : InputVector2Simple
    {
        public InputVector2[] Children { get; } = children;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            if (Children.Length == 0)
            {
                return Vector2.Zero;
            }

            Vector2 v = Children[0].GetState(state.Updater).Value;
            for (int i = 1; i < Children.Length; i++)
            {
                v *= Children[i].GetState(state.Updater).Value;
            }

            return v;
        }

        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return string.Join(" x ", Children.Select(f => f.ToString()));
        }
    }
}
