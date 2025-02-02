namespace BytingLib
{
    public class IntConst : IntInput
    {
        private readonly int value;

        public IntConst(int value)
        {
            this.value = value;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        protected override int CalculateValue(FullInput input)
        {
            return value;
        }
    }
}
