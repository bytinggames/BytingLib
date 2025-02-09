using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsInputRecordings
    {
        #region >InputInputRecordings

        public InputBool StartRecording { get; } = new BoolAnd(Keys.LeftShift, Keys.F5);
        public InputBool StopRecording { get; } = new BoolAnd(new BoolNot(Keys.LeftShift), Keys.F5);

        #endregion
    }

}
