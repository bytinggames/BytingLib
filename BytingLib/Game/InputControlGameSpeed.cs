using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputControlGameSpeed : Input
    {
        public BoolOutput SpeedUp10 { get; } = Keys.LeftAlt;
        public BoolOutput SpeedUp100 { get; } = new(new BoolAnd(new BoolAnd(Keys.LeftAlt, Keys.Apps)));
        public BoolOutput Halt { get; } = Keys.Apps;
        public BoolOutput ForwardOneFrame { get; } = /*new(new BoolAnd(Keys.Apps,*/ Keys.LeftAlt;/*));*/

        public InputControlGameSpeed(InputUpdater updater) : base(updater)
        {
        }
    }
}
