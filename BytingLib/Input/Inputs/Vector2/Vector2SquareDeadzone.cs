
namespace BytingLib
{
    public class Vector2SquareDeadzone(float deadZoneInner, float deadZoneOuter, InputVector2 child) : InputVector2Simple
    {
        public float DeadZoneInner { get; } = deadZoneInner;
        public float DeadZoneOuter { get; } = deadZoneOuter;
        public InputVector2 Child { get; } = child;

        protected override Vector2 CalculateValue(FullInput fullInput, InputVector2State state)
        {
            Vector2 input = Child.GetState(state.Updater).Value;
            input.X = ApplySingleAxisDeadzone(input.X, DeadZoneInner, DeadZoneOuter);
            input.Y = ApplySingleAxisDeadzone(input.Y, DeadZoneInner, DeadZoneOuter);
            return input;
        }

        private static float ApplySingleAxisDeadzone(float input, float deadzoneInner, float deadzoneOuter)
        {
            return MathF.Abs(input).MapRangeClamped(deadzoneInner, deadzoneOuter, 0.0f, 1.0f)
                * Math.Sign(input); // Restore sign to the axis
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }
    }
}
