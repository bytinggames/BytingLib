namespace BytingLib
{
    public class BindVector2
    {
        public Flag BindFlag { get; set; }
        public float? MouseSpeedFactor { get; set; }
        public BindKey? Left { get; set; }
        public BindKey? Right { get; set; }
        public BindKey? Up { get; set; }
        public BindKey? Down { get; set; }
        public Func<Vector2, Vector2>? ManipulateLeftStick { get; set; }
        public Func<Vector2, Vector2>? ManipulateRightStick { get; set; }

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
            if (IsFlag(Flag.MouseCustom))
            {
                inputDir += input.GetCustomMouseMovement() * (MouseSpeedFactor ?? 1f);
            }
            if (IsFlag(Flag.GamePadLeftStick))
            {
                AddGamePadStickMovement(ref inputDir, input.GamePad.LeftThumbStick, ManipulateLeftStick);
            }
            if (IsFlag(Flag.GamePadRightStick))
            {
                AddGamePadStickMovement(ref inputDir, input.GamePad.RightThumbStick, ManipulateRightStick);
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

        private void AddGamePadStickMovement(ref Vector2 inputDir, Vector2 move, Func<Vector2, Vector2>? manipulateStick)
        {
            if (manipulateStick != null)
            {
                move = manipulateStick(move);
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
            MouseCustom = 8,
            GamePadLeftStick = 16,
            GamePadRightStick = 32,
        }
    }
}