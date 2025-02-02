namespace BytingLib
{
    public class BoolConst : BoolInput
    {
        private readonly bool value;

        public BoolConst(bool value)
        {
            this.value = value;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        protected override bool CalculateValue(FullInput input)
        {
            return value;
        }
    }
}
