namespace BytingLib
{
    public class Vector4FuncValue(Func<Vector4> getValue) : InputVector4Simple
    {
        private readonly Func<Vector4> getValue = getValue;

        protected override Vector4 CalculateValue(FullInput input, InputVector4State state)
        {
            return getValue();
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield break;
        }
    }

}
