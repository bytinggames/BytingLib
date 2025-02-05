namespace BytingLib
{
    public class InputFloatState : InputState<float>
    {
        public InputFloatState(InputUpdater updater)
            :base(updater)
        {
        }

        protected long stamp;

        public float LastValue { get; private set; }
        public float Value { get; private set; }
        public float Delta => Updater.CurrentStamp >= 2 ? Value - LastValue : 0f;

        public override void Update(float value)
        {
            LastValue = Value;
            Value = value;
        }

        public static implicit operator float(InputFloatState f) => f.Value;
    }
}
