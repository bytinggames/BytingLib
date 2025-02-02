using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BoolAnyInput : BoolInput
    {
        KeyboardState lastKeyState;
        MouseState lastMouseState;
        GamePadState lastGamePadState;

        public override IEnumerable<InputUpdate> GetChildren()
        {
            yield break;
        }

        public override bool IsDown(FullInput input)
        {
            bool inputChanged = input.MouseState != lastMouseState
                || input.KeyState != lastKeyState
                || input.GamePadState != lastGamePadState;

            lastKeyState = input.KeyState;
            lastMouseState = input.MouseState;
            lastGamePadState = input.GamePadState;

            return inputChanged;
        }
    }
}
