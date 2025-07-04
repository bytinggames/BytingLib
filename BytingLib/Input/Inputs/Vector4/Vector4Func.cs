namespace BytingLib
{
    public class Vector4Func(Func<InputVector4> getVector2) : InputVector4Simple
    {
        private readonly Func<InputVector4> getVector2 = getVector2;

        protected override Vector4 CalculateValue(FullInput input, InputVector4State state)
        {
            return getVector2().GetState(state.Updater);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return getVector2();
        }
    }

}
