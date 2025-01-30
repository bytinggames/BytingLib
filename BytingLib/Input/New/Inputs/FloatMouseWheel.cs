namespace BytingLib
{
    public class FloatMouseWheel : FloatInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override float GetValue(FullInput input)
        {
            return input.MouseState.ScrollWheelValue;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
