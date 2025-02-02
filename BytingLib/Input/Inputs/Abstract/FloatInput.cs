namespace BytingLib
{
    public abstract class FloatInput : InputUpdate
    {
        public float LastValue { get; private set; }
        public float Value { get; private set; }
        public float Delta => updater.CurrentStamp >= 2 ? Value - LastValue : 0f;

        public abstract float CalculateValue(FullInput input);

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
