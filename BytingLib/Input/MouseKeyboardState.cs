
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public struct MouseKeyboardState
    {
        public MouseState MouseState { get; }
        public KeyboardState KeyState { get; }
        public bool IsActivatedThisUpdate { get; }

        public MouseKeyboardState(MouseState mouseState, KeyboardState keyState, bool isActivatedThisUpdate)
        {
            MouseState = mouseState;
            KeyState = keyState;
            IsActivatedThisUpdate = isActivatedThisUpdate;
        }
    }
}
