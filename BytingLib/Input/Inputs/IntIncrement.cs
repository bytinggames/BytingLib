namespace BytingLib
{
    public class IntIncrement : IntInput
    {
        private readonly IntInput input;
        private readonly BoolInput increment;
        private readonly BoolInput? decrement;

        public IntIncrement(IntInput input, BoolInput increment, BoolInput? decrement)
        {
            this.input = input;
            this.increment = increment;
            this.decrement = decrement;
        }

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield return input;
            yield return increment;
            if (decrement != null)
            {
                yield return decrement;
            }
        }

        protected override int CalculateValue(FullInput input)
        {
            int value = this.input.Value;
            if (increment.Pressed)
            {
                value++;
            }
            if (decrement != null && decrement.Pressed)
            {
                value--;
            }
            return value;
        }
    }
}
