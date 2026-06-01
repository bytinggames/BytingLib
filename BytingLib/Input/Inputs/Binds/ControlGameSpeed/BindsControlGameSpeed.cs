using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsControlGameSpeed
    {
        #region >InputControlGameSpeed

        public InputBool SpeedUp10 { get; } = Keys.LeftAlt;
        public InputBool SpeedUp100 { get; } = new BoolAnd(Keys.LeftAlt, Keys.Apps);
        public InputBool Halt { get; } = new BoolOr(Keys.Apps, Keys.Decimal);
        public InputBool ForwardOneFrame { get; } = /*new(new BoolAnd(Keys.Apps,*/ Keys.LeftAlt;/*));*/

        #endregion
    }
}
