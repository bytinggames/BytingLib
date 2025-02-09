namespace BytingLib
{
    public class InputIntState : InputState<int>
    {
        public InputIntState(InputUpdater updater)
            :base(updater)
        {
        }

        protected long stamp;

        public int LastValue { get; private set; }
        public int Value { get; private set; }
        public int Delta => Updater.CurrentStamp >= 2 ? Value - LastValue : 0;

        public override void Update(int value)
        {
            LastValue = Value;
            Value = value;
        }

        public static implicit operator int(InputIntState f) => f.Value;
    }
}
