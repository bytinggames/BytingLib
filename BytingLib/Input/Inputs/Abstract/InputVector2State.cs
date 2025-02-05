namespace BytingLib
{
    public class InputVector2State : InputState<Vector2>
    {
        public InputVector2State(InputUpdater updater)
            :base(updater)
        {
        }

        protected long stamp;

        public Vector2 LastValue { get; private set; }
        public Vector2 Value { get; private set; }
        public Vector2 Delta => Updater.CurrentStamp >= 2 ? Value - LastValue : Vector2.Zero;
        public float X => Value.X;
        public float Y => Value.Y;

        public override void Update(Vector2 value)
        {
            LastValue = Value;
            Value = value;
        }

        public static implicit operator Vector2(InputVector2State f) => f.Value;
    }
}
