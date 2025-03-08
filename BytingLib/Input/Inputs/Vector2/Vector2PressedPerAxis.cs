namespace BytingLib
{
    public class Vector2PressedPerAxis(InputVector2 child) : InputVector2Simple
    {
        public float Threshold { get; set; } = 0.5f;

        public InputVector2 Child { get; } = child;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 output = Vector2.Zero;
            var childState = Child.GetState(state.Updater);
            if (MathF.Abs(childState.LastValue.X) < Threshold
                && MathF.Abs(childState.Value.X) >= Threshold)
            {
                output.X = childState.Value.X;
            }
            if (MathF.Abs(childState.LastValue.Y) < Threshold
                && MathF.Abs(childState.Value.Y) >= Threshold)
            {
                output.Y = childState.Value.Y;
            }
            return output;
        }

        public override string ToString()
        {
            return $" InputVector2PressedState( {Child} )";
        }
    }
}
