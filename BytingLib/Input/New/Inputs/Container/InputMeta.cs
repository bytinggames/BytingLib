using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputMeta : Input
    {
        public BoolOutput ToggleFullscreen { get; } = Keys.F11;
        public BoolOutput Screenshot { get; } = new(And(Not(Ctrl()), Keys.F12));
        public BoolOutput ScreenshotDelayed { get; } = new(Shift());
        public BoolOutput Escape { get; } = Keys.Escape;

#if DEBUG
        public BoolOutput SwapScreen { get; } = Keys.Tab;
#else
        public BoolOutput SwapScreen { get; } = new(And(Ctrl(), Keys.Tab));

#endif

        public InputMeta(InputUpdater updater) : base(updater)
        {
        }
    }
}
