namespace BytingLib
{
    public class InputBoolState : InputState<bool>
    {
        public InputBoolState(InputUpdater updater)
            :base(updater)
        {
        }

        protected long stamp;

        public bool Down => stamp <= 0 ? false : (Updater.CurrentStamp - stamp) >= 0;
        public bool Pressed => stamp <= 0 ? false : Updater.CurrentStamp == stamp;
        public int DownTime => stamp <= 0 ? 0 : (int)(Updater.CurrentStamp - stamp + 1);
        public bool Released => Updater.CurrentStamp > 0 && - stamp == Updater.CurrentStamp;
        public int ReleasedTime => stamp >= 0 ? 0 : (int)(Updater.CurrentStamp - -stamp + 1);

        public override void Update(bool isDown)
        {
            if (stamp <= 0)
            {
                // currently not held
                if (isDown)
                {
                    // beginning to hold
                    stamp = Updater.CurrentStamp;
                }
            }
            else
            {
                // currently held
                if (!isDown)
                {
                    // hold ending
                    stamp = -Updater.CurrentStamp;
                }
            }
        }

        public static implicit operator bool(InputBoolState f) => f.Down;
    }
}
