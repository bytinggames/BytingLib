
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public struct MouseKeyboardState
    {
        public MouseState MouseState { get; }
        public KeyboardState KeyState { get; }

        public MouseKeyboardState(MouseState mouseState, KeyboardState keyState)
        {
            MouseState = mouseState;
            KeyState = keyState;
        }
    }
}
