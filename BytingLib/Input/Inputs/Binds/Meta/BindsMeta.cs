using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsMeta : InputBinds
    {
        #region >InputMeta

        public InputBool Screenshot { get; } = And(Not(Keys.U), Keys.F12);
        public InputBool ScreenshotAndCopy { get; } = And(Keys.LeftControl, Keys.C);
        public InputBool ScreenshotAndCopyFile { get; } = And(Keys.RightControl, Keys.C);
        public InputBool ScreenshotDelayed { get; } = Shift();
        public InputBool ToggleFullscreen { get; } = Keys.F11;
        public InputBool InterruptReplay { get; } = Keys.Escape;
        public InputBool SwapScreen { get; } =
#if DEBUG
            Keys.Tab;
#else
            And(Ctrl(), Keys.Tab);
#endif
        #endregion
    }
}
