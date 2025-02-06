namespace BytingLib
{
    public class Vector2MaxLength(params InputVector2[] children) : InputVector2Simple
    {
        public InputVector2[] Children { get; } = children;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 max = Vector2.Zero;
            float maxLength = 0f;
            for (int i = 0; i < Children.Length; i++)
            {
                Vector2 val = Children[i].GetState(state.Updater).Value;
                float length = val.LengthSquared();
                if (length > maxLength)
                {
                    maxLength = length;
                    max = val;
                }
            }
            return max;
        }

        public override IEnumerable<Input> GetChildren()
        {
            return Children;
        }

        public override string ToString()
        {
            return $"Max Length ({string.Join(' ', Children.Select(f => f.ToString()))})";
        }
    }
}
