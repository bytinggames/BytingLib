namespace BytingLib
{
    public class Vector4MaxLength(params InputVector4[] children) : InputVector4Simple
    {
        public InputVector4[] Children { get; } = children;

        protected override Vector4 CalculateValue(FullInput input, InputVector4State state)
        {
            Vector4 max = Vector4.Zero;
            float maxLength = 0f;
            for (int i = 0; i < Children.Length; i++)
            {
                Vector4 val = Children[i].GetState(state.Updater).Value;
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
