namespace BytingLib
{
    public abstract class IntInput : InputUpdate
    {
        public int LastValue { get; private set; }
        public int Value { get; private set; }
        public int Delta => updater.CurrentStamp >= 2 ? Value - LastValue : 0;

        public abstract int CalculateValue(FullInput input);

        public override void Update(FullInput input)
        {
            LastValue = Value;
            Value = CalculateValue(input);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
