namespace BytingLib
{
    public class InputVector2PressedState : InputVector2State
    {
        public long? StampPressed { get; set; }

        public InputVector2PressedState(InputUpdater updater) : base(updater)
        {
        }
    }

    public class Vector2PressedCircular(InputVector2 Child, bool HoldToRepeat) : InputVector2<InputVector2PressedState>
    {
        public InputVector2 Child { get; } = Child;
        public bool HoldToRepeat { get; set; } = HoldToRepeat;
        public HoldToRepeat? HoldToRepeatInstance { get; set; } = HoldToRepeat ? new HoldToRepeat() : null;
        public float ThresholdActiveSquared { get; set; } = MathF.Pow(0.6f, 2f);
        public float ThresholdInactiveSquared { get; set; } = MathF.Pow(0.4f, 2f);


        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2PressedState state)
        {
            var childState = Child.GetState(state.Updater);
            if (childState.LastValue.LengthSquared() < ThresholdInactiveSquared)
            {
                // deactivate
                state.StampPressed = null;
            }
            else if (childState.Value.LengthSquared() >= ThresholdActiveSquared)
            {
                if (state.StampPressed == null) // not pressed yet?
                {
                    // pressed
                    state.StampPressed = state.Updater.CurrentStamp;
                    return Vector2.Normalize(childState.Value);
                }
                else if (HoldToRepeatInstance != null)
                {
                    // held
                    if (HoldToRepeatInstance.HoldIsRepeat(state.Updater.CurrentStamp, state.StampPressed.Value))
                    {
                        return Vector2.Normalize(childState.Value);
                    }
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
