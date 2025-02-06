namespace BytingLib
{
    public class Vector2GamePadStick(bool? leftOrRight) : InputVector2Simple
    {
        public bool? LeftOrRight { get; } = leftOrRight;

        protected override Vector2 CalculateValue(FullInput input, InputVector2State state)
        {
            Vector2 v;
            if (LeftOrRight == null)
            {
                v = input.GamePadState.ThumbSticks.Left + input.GamePadState.ThumbSticks.Right;
            }
            else if (LeftOrRight.Value)
            {
                v = input.GamePadState.ThumbSticks.Left;
            }
            else
            {
                v = input.GamePadState.ThumbSticks.Right;
            }
            // invert y so it matches mouse position axes
            v.Y = -v.Y;
            return v;
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
