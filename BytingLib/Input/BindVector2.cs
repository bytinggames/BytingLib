namespace BytingLib
{
    public class BindVector2
    {
        public Flag BindFlag { get; set; }
        public float? MouseSpeedFactor { get; set; }
        public float? GamePadStickSpeedFactor { get; set; }
        public float? GamePadStickPow { get; set; }
        public BindKey? Left { get; set; }
        public BindKey? Right { get; set; }
        public BindKey? Up { get; set; }
        public BindKey? Down { get; set; }

        public BindVector2(Flag bindFlag)
        {
            BindFlag = bindFlag;
        }
        public BindVector2(Flag bindFlag, BindKey left, BindKey right, BindKey up, BindKey down)
        {
            BindFlag = bindFlag;
            Left = left;
            Right = right;
            Up = up;
            Down = down;
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
                if (input.Keys.D.Down)
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
                inputDir += input.Mouse.Move * (MouseSpeedFactor ?? 1f);
            }
            if (IsFlag(Flag.Mouse3D))
            {
                inputDir += input.GetCustomMouseMovement() * (MouseSpeedFactor ?? 1f);
            }
            if (IsFlag(Flag.GamePadLeftStick))
            {
                AddGamePadStickMovment(ref inputDir, input.GamePad.LeftThumbStick);
            }
            if (IsFlag(Flag.GamePadRightStick))
            {
                AddGamePadStickMovment(ref inputDir, input.GamePad.RightThumbStick);
            }

            if (Left != null && Left.GetKey(input).Down)
            {
                inputDir.X--;
            }
            if (Right != null && Right.GetKey(input).Down)
            {
                inputDir.X++;
            }
            if (Up != null && Up.GetKey(input).Down)
            {
                inputDir.Y--;
            }
            if (Down != null && Down.GetKey(input).Down)
            {
                inputDir.Y++;
            }

            return inputDir;
        }

        private void AddGamePadStickMovment(ref Vector2 inputDir, Vector2 move)
        {
            if (GamePadStickPow != null && GamePadStickPow != 1f)
            {
                move = move.GetSign() * new Vector2(MathF.Pow(move.X, GamePadStickPow.Value), MathF.Pow(move.Y, GamePadStickPow.Value)).GetAbs();
            }
            if (GamePadStickSpeedFactor != null)
            {
                move *= GamePadStickSpeedFactor.Value;
            }

            inputDir += move;
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