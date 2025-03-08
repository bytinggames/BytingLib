namespace BytingLib
{
    public class InputVector2PressedState : InputVector2State
    {
        public bool Active { get; set; }

        public InputVector2PressedState(InputUpdater updater) : base(updater)
        {
        }
    }

    public class Vector2PressedCircular(InputVector2 child) : InputVector2<InputVector2PressedState>
    {
        public InputVector2 Child { get; } = child;

        public float ThresholdActiveSquared { get; set; } = MathF.Pow(0.6f, 2f);
        public float ThresholdInactiveSquared { get; set; } = MathF.Pow(0.4f, 2f);

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2PressedState state)
        {
            // check if current input goes above threshold
            var childState = Child.GetState(state.Updater);
            if (state.Active)
            {
                if (childState.LastValue.LengthSquared() < ThresholdInactiveSquared)
                {
                    state.Active = false;
                }
            }
            else
            {
                if (childState.Value.LengthSquared() >= ThresholdActiveSquared)
                {
                    state.Active = true;
                    return Vector2.Normalize(childState.Value);
                }
            }

            return Vector2.Zero;
        }

        public override string ToString()
        {
            return $" Vector2PressedCircular ( {Child} )";
        }

        public override InputVector2PressedState CreateState(InputUpdater updater)
        {
            return new InputVector2PressedState(updater);
        }
    }
}
