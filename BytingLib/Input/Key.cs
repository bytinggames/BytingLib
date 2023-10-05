﻿namespace BytingLib
{
    // this is a struct and no class, because a reference to this key should not be kept if you want to have that key updated every frame.
    // That key is generated, when it's requested.
    public struct Key : IKey
    {
        public bool Down { get; } = false;
        public bool Pressed { get; } = false;
        public bool Released { get; } = false;

        public Key(bool down, bool toggled)
        {
            Down = down;
            if (toggled)
            {
                if (down)
                    Pressed = true;
                else
                    Released = true;
            }
        }

        public IKey Or(IKey key)
        {
            bool downNow = Down || key.Down;
            bool toggle;
            if (downNow)
            {
                toggle = Pressed && key.Pressed
                    || Pressed && !key.Down
                    || key.Pressed && !Down;
            }
            else
            {
                toggle = Released && !key.Down
                    || key.Released && !Down;
            }
            return new Key(downNow, toggle);
        }
    }
}