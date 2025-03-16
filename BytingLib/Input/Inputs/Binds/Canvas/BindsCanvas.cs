
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsCanvas : InputBinds
    {
        #region >InputCanvas

        public InputVector2 MousePosition { get; set; } = new Vector2Mouse();
        public InputBool Click { get; set; } = MouseButton.Left;
        public InputInt Scroll { get; set; } = new IntMouseWheel();
        public InputBool Enter { get; set; } = Or(Keys.Enter, Keys.Space);
        public InputVector2 Navigate { get; set; } = new Vector2PressedPerAxis(Arrows, true);
        public InputVector2 NavigateWithLetters { get; set; } = new Vector2PressedPerAxis(Wasd, true);

        #endregion

        protected override void SetControllerBinds()
        {
            Scroll = new IntIncrement(new IntMouseWheel(), Buttons.RightShoulder, Buttons.LeftShoulder);
            Enter = Or(Keys.Enter, Keys.Space, Buttons.A);
            Navigate = new Vector2MaxLength(
                new Vector2PressedPerAxis(ArrowsOrDpad, true),
                new Vector2PressedCircular(new Vector2GamePadStick(null), true));

            base.SetControllerBinds();
        }
    }
}
