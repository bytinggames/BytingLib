namespace BytingLib
{
    public class IntMouseWheel : IntInput
    {
        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override int GetValue(FullInput input)
        {
            return input.MouseState.ScrollWheelValue;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
