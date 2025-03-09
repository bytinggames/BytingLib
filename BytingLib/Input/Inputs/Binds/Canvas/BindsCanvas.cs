
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsCanvas : InputBinds
    {
        #region >InputCanvas

        public InputVector2 MousePosition { get; set; } = new Vector2Mouse();
        public InputBool Click { get; set; } = MouseButton.Left;
        public InputInt Scroll { get; set; } = new IntMouseWheel();
        public InputBool Enter { get; set; } = Or(Keys.Enter, Keys.Space, Buttons.A, Buttons.Start);
        public InputVector2 Navigate { get; set; } = new Vector2MaxLength(
            new Vector2PressedPerAxis(ArrowsOrWasdOrDpad, true), 
            new Vector2PressedCircular(new Vector2GamePadStick(null), true));

        #endregion
    }
}
