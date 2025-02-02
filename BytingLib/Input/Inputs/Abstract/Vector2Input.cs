namespace BytingLib
{
    public abstract class Vector2Input : InputUpdate
    {
        public Vector2? LastValue { get; private set; }
        public Vector2 Value { get; private set; }
        public Vector2 Delta => LastValue.HasValue ? Value - LastValue.Value : Vector2.Zero;
        public float X => Value.X;
        public float Y => Value.Y;

        public abstract Vector2 GetValue(FullInput input);

        public override void Update(FullInput input)
        {
            LastValue = Value;
            Value = GetValue(input);
        }
    }
}
