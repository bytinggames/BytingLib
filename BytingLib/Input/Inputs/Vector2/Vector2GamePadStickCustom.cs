
namespace BytingLib
{
    public class Vector2GamePadStickCustom : InputVector2Simple
    {
        private readonly InputVector2 child;

        public bool? LeftOrRight { get; }
        public bool InvertX { get; }
        public bool InvertY { get; }
        public float DeadZoneInner { get; }
        public float DeadZoneOuter { get; }
        public float CurveExponent { get; }
        public float Sensitivity { get; }

        const float SpeedFactor = 50f;

        public Vector2GamePadStickCustom(
            bool? leftOrRight,
            bool invertX = false,
            bool invertY = false,
            float deadZoneInner = 0.05f,
            float deadZoneOuter = 0.98f,
            float curveExponent = 1.3f,
            float sensitivity = 1f)
        {
            LeftOrRight = leftOrRight;
            InvertX = invertX;
            InvertY = invertY;
            DeadZoneInner = deadZoneInner;
            DeadZoneOuter = deadZoneOuter;
            CurveExponent = curveExponent;
            Sensitivity = sensitivity;

            child = new Vector2GamePadStick(leftOrRight);
            child = new Vector2CircularDeadzone(DeadZoneInner, DeadZoneOuter, child);
            child = new Vector2StickPow(CurveExponent, child);
            child = new Vector2Multiply(SpeedFactor * Sensitivity, child);
        }

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 val = child.GetState(state.Updater).Value;

            if (InvertX)
            {
                val.X = -val.X;
            }
            if (InvertY)
            {
                val.Y = -val.Y;
            }

            return val;
        }

        public override IEnumerable<Input> GetChildren()
        {
            yield return child;
        }

        public override string ToString()
        {
            if (LeftOrRight == null)
            {
                return "Any Thumb Stick";
            }
            else if (LeftOrRight.Value)
            {
                return $"Left Thumb Stick";
            }
            else
            {
                return $"Right Thumb Stick";
            }
        }
    }
}