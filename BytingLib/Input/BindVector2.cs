namespace BytingLib
{
    public class BindVector2
    {
        public Flag BindFlag { get; set; }

        public BindVector2(Flag bindFlag)
        {
            BindFlag = bindFlag;
        }

        public Vector2 GetVector2(AllInput input)
        {
            Vector2 inputDir = Vector2.Zero;

            if (IsFlag(Flag.WASD))
            {
                if (input.Keys.W.Down)
                {
                    inputDir.Y--;
                }
                if (input.Keys.S.Down)
                {
                    inputDir.Y++;
                }
                if (input.Keys.F.Down)
                {
                    inputDir.X++;
                }
                if (input.Keys.A.Down)
                {
                    inputDir.X--;
                }
            }
            if (IsFlag(Flag.ArrowKeys))
            {
                if (input.Keys.Up.Down)
                {
                    inputDir.Y--;
                }
                if (input.Keys.Down.Down)
                {
                    inputDir.Y++;
                }
                if (input.Keys.Right.Down)
                {
                    inputDir.X++;
                }
                if (input.Keys.Left.Down)
                {
                    inputDir.X--;
                }
            }
            if (IsFlag(Flag.Mouse2D))
            {
                inputDir += input.Mouse.Move;
            }
            if (IsFlag(Flag.Mouse3D))
            {
                inputDir += input.GetCustomMouseMovement();
            }
            if (IsFlag(Flag.GamePadLeftStick))
            {
                inputDir += input.GamePad.LeftThumbStick;
            }
            if (IsFlag(Flag.GamePadRightStick))
            {
                inputDir += input.GamePad.RightThumbStick;
            }

            return inputDir;
        }

        private bool IsFlag(Flag flag)
        {
            return (BindFlag & flag) == flag;
        }

        [Flags]
        public enum Flag
        {
            None = 0,
            WASD = 1,
            ArrowKeys = 2,
            Mouse2D = 4,
            Mouse3D = 8,
            GamePadLeftStick = 16,
            GamePadRightStick = 32,
        }
    }
}