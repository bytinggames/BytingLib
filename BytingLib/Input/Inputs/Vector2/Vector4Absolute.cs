namespace BytingLib
{
    public class Vector4Absolute(InputVector2 child) : InputVector4Simple
    {
        public InputVector2 Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector4 CalculateValue(FullInput input, InputVector4State state)
        {
            return Vector4Extension.Absolute(Child.GetState(state.Updater).Value);
        }

        public override string ToString()
        {
            return " Absolute " + Child;
        }
    }
}
