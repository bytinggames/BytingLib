namespace BytingLib
{
    public class IntIf(BoolInput condition, IntInput input, int defaultValue = -1) : IntInput
    {
        private readonly BoolInput condition = condition;
        private readonly IntInput input = input;
        private readonly int defaultValue = defaultValue;

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return condition;
            yield return input;
        }

        public override int GetValue(FullInput input)
        {
            if (condition.Down)
            {
                return this.input.Value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
