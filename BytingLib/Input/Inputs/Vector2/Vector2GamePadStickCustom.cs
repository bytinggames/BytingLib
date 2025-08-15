
namespace BytingLib
{
    public class Vector2GamePadStickCustom : InputVector2Simple
    {
        private readonly InputVector2 child;

        public bool? LeftOrRight { get => stick.LeftOrRight; set => stick.LeftOrRight = value; }
        public bool InvertX { get; set; }
        public bool InvertY { get; set; }
        public float DeadZoneInner { get => deadZone.DeadZoneInner; set => deadZone.DeadZoneInner = value; }
        public float DeadZoneOuter { get => deadZone.DeadZoneOuter; set => deadZone.DeadZoneOuter = value; }
        public float CurveExponent { get => pow.CurveExponent; set => pow.CurveExponent = value; }
        public float SensitivityX { get => multiply.Factor.X / SpeedFactor; set => multiply.Factor = new Vector2(value* SpeedFactor, multiply.Factor.Y); }
        public float SensitivityY { get => multiply.Factor.Y / SpeedFactor; set => multiply.Factor = new Vector2(multiply.Factor.X, value * SpeedFactor); }

        const float SpeedFactor = 25f;


        private readonly Vector2GamePadStick stick;
        private readonly Vector2CircularDeadzone deadZone;
        private readonly Vector2StickPow pow;
        private readonly Vector2Multiply2 multiply;

        public Vector2GamePadStickCustom(
            bool? leftOrRight,
            bool invertX = false,
            bool invertY = false,
            float deadZoneInner = 0.2f,
            float deadZoneOuter = 0.94f,
            float curveExponent = 1.3f,
            float sensitivityX = 1f,
            float sensitivityY = 1f)
        {
            InvertX = invertX;
            InvertY = invertY;

            child = stick = new Vector2GamePadStick(leftOrRight);
            child = deadZone = new Vector2CircularDeadzone(deadZoneInner, deadZoneOuter, child);
            child = pow = new Vector2StickPow(curveExponent, child);
            child = multiply = new Vector2Multiply2(SpeedFactor * new Vector2(sensitivityX, sensitivityY), child);
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

        public void ResetToDefault()
        {
            Vector2GamePadStickCustom defaultBind = new(LeftOrRight);

            CurveExponent = defaultBind.CurveExponent;
            DeadZoneInner = defaultBind.DeadZoneInner;
            DeadZoneOuter = defaultBind.DeadZoneOuter;
            InvertX = defaultBind.InvertX;
            InvertY = defaultBind.InvertY;
            SensitivityX = defaultBind.SensitivityX;
            SensitivityY = defaultBind.SensitivityY;
        }
    }
}