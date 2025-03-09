namespace BytingLib
{
    public class Vector2PressedPerAxisState : InputVector2State
    {
        public long?[] StampPressed { get; set; } = new long?[2]; // for x and y

        public Vector2PressedPerAxisState(InputUpdater updater) : base(updater)
        {
        }
    }

    public class Vector2PressedPerAxis(InputVector2 child, bool holdToRepeat) : InputVector2<Vector2PressedPerAxisState>
    {
        public InputVector2 Child { get; } = child;
        public HoldToRepeat? HoldToRepeat { get; set; } = holdToRepeat ? new HoldToRepeat() : null;
        public float ThresholdActive { get; set; } = 0.6f;
        public float ThresholdInactive { get; set; } = 0.4f;

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }

        protected override Vector2 CalculateValue(FullInput input, Vector2PressedPerAxisState state)
        {
            var childState = Child.GetState(state.Updater);
            float[] value = [childState.Value.X, childState.Value.Y];
            float[] lastValue = [childState.LastValue.X, childState.LastValue.Y];
            float[] output = new float[2];

            for (int i = 0; i < 2; i++)
            {
                if (MathF.Abs(value[i]) < ThresholdInactive)
                {
                    // deactivate
                    state.StampPressed[i] = null;
                }
                else if (MathF.Abs(value[i]) >= ThresholdActive)
                {
                    if (state.StampPressed[i] == null // not pressed yet?
                        || MathF.Sign(value[i]) != MathF.Sign(lastValue[i])) // or instantly swap to other side of the axis
                    {
                        // pressed
                        state.StampPressed[i] = state.Updater.CurrentStamp;
                        output[i] = value[i];
                    }
                    else if (HoldToRepeat != null)
                    {
                        // held
                        if (HoldToRepeat.HoldIsRepeat(state.Updater.CurrentStamp, state.StampPressed[i]!.Value))
                        {
                            output[i] = MathF.Sign(value[i]);
                        }
                    }
                }
            }
            return new Vector2(output[0], output[1]);
        }

        public override string ToString()
        {
            return $" InputVector2PressedState( {Child} )";
        }

        public override Vector2PressedPerAxisState CreateState(InputUpdater updater)
        {
            return new Vector2PressedPerAxisState(updater);
        }
    }
}
