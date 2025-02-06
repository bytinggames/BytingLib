using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsMeta : InputBinds
    {
        #region >InputMeta

        public InputBool Screenshot { get; } = And(Not(Ctrl()), Keys.F12);
        public InputBool ScreenshotDelayed { get; } = Shift();
        public InputBool ToggleFullscreen { get; } = Keys.F11;
        public InputBool InterruptReplay { get; } = Keys.Escape;
        public InputBool SwapScreen { get; } = And(Ctrl(), Keys.Tab);

        #endregion
    }
}
