using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class BindsMeta : InputBinds
    {
        #region >InputMeta

        public InputBool Screenshot { get; } = Modify(Keys.F12);
        public InputBool ScreenshotAndCopy { get; } = Modify(Keys.C, true);
        public InputBool ScreenshotAndCopyFile { get; } = Modify(Keys.C, true, false, true);
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
