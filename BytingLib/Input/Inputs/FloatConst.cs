namespace BytingLib
{
    public class FloatConst : FloatInput
    {
        private readonly float value;

        public FloatConst(float value)
        {
            this.value = value;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override float GetValue(FullInput input)
        {
            return value;
        }
    }
}
