namespace BytingLib
{
    public class Vector4Sum(params InputVector4[] children) : InputVector4Simple
    {
        public InputVector4[] Children { get; } = children;

        protected override Vector4 CalculateValue(FullInput input, InputVector4State state)
        {
            Vector4 sum = Vector4.Zero;
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
            return $"Sum ({string.Join(' ', Children.Select(f => f.ToString()))})";
        }
    }
}
