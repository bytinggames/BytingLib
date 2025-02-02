using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class BoolInput : InputUpdate, IBoolDelta
    {
        protected long stamp;

        public bool Down => stamp <= 0 ? false : (updater.CurrentStamp - stamp) >= 0;
        public bool Pressed => stamp <= 0 ? false : updater.CurrentStamp == stamp;
        public int DownTime => stamp <= 0 ? 0 : (int)(updater.CurrentStamp - stamp + 1);
        public bool Released => -stamp == updater.CurrentStamp;
        public int ReleasedTime => stamp >= 0 ? 0 : (int)(updater.CurrentStamp - -stamp + 1);

        protected abstract bool CalculateValue(FullInput input);

        public override void Update(FullInput input)
        {
            bool isDown = CalculateValue(input);
            if (stamp <= 0)
            {
                // currently not held
                if (isDown)
                {
                    // beginning to hold
                    stamp = updater.CurrentStamp;
                }
            }
            else
            {
                // currently held
                if (!isDown)
                {
                    // hold ending
                    stamp = -updater.CurrentStamp;
                }
            }
        }

        public static implicit operator BoolInput(Keys key) => new BoolKey(key);
        public static implicit operator BoolInput(MouseButton mouseButton) => new BoolMouse(mouseButton);
        public static implicit operator BoolInput(Buttons gamePadButton) => new BoolGamePad(gamePadButton);
    }
}
