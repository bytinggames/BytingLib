using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputNumbers : Input
    {
        public InputNumbers(InputUpdater updater) : base(updater)
        {
        }

        public BoolOutput D0 { get; } = new(new BoolKey(Keys.D0));
        public BoolOutput D1 { get; } = new(new BoolKey(Keys.D1));
        public BoolOutput D2 { get; } = new(new BoolKey(Keys.D2));
        public BoolOutput D3 { get; } = new(new BoolKey(Keys.D3));
        public BoolOutput D4 { get; } = new(new BoolKey(Keys.D4));
        public BoolOutput D5 { get; } = new(new BoolKey(Keys.D5));
        public BoolOutput D6 { get; } = new(new BoolKey(Keys.D6));
        public BoolOutput D7 { get; } = new(new BoolKey(Keys.D7));
        public BoolOutput D8 { get; } = new(new BoolKey(Keys.D8));
        public BoolOutput D9 { get; } = new(new BoolKey(Keys.D9));

        public IBoolDelta Number(int n)
        {
            return n switch
            {
                0 => D0,
                1 => D1,
                2 => D2,
                3 => D3,
                4 => D4,
                5 => D5,
                6 => D6,
                7 => D7,
                8 => D8,
                9 => D9,
                _ => new BoolFalse(),
            };
        }
    }
}
