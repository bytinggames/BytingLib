using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputRecordingBinds : Input
    {
        public BoolOutput StartRecording { get; } = new(new BoolAnd(Keys.LeftShift, Keys.F5));
        public BoolOutput StopRecording { get; } = new(new BoolAnd(new BoolNot(Keys.LeftShift), Keys.F5));

        public InputRecordingBinds(InputUpdater updater) : base(updater)
        {
        }
    }

}
