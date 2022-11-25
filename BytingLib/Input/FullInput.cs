﻿
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public struct FullInput
    {
        public MouseState MouseState { get; }
        public KeyboardState KeyState { get; }
        public GamePadState GamePadState { get; }
        public MetaInputState MetaState { get; }

        public FullInput(MouseState mouseState, KeyboardState keyState, GamePadState gamePad, MetaInputState metaState)
        {
            MouseState = mouseState;
            KeyState = keyState;
            GamePadState = gamePad;
            MetaState = metaState;
        }
    }
}
