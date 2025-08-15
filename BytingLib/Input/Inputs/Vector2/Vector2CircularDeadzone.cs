
namespace BytingLib
{
    public class Vector2CircularDeadzone(float deadZoneInner, float deadZoneOuter, InputVector2 child) : InputVector2Simple
    {
        public float DeadZoneInner { get; set; } = deadZoneInner;
        public float DeadZoneOuter { get; set; } = deadZoneOuter;
        public InputVector2 Child { get; } = child;

        protected override Vector2 CalculateValue(FullInput fullInput, InputVector2State state)
        {
            Vector2 input = Child.GetState(state.Updater).Value;
            float isLength = input.Length();
            float shouldLength = isLength.MapRangeClamped(DeadZoneInner, DeadZoneOuter, 0.0f, 1.0f);
            if (isLength == 0) // Prevent div by zero
            {
                return Vector2.Zero;
            }
            return input * (shouldLength / isLength);
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return Child;
        }
    }
}
