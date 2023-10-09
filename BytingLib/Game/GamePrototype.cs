using BytingLib.Markup;
using BytingLib.Serialization;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GamePrototype : GameBase
    {
        protected readonly GameSpeed updateSpeed, drawSpeed;
        /// <summary>Used for creating markup elements</summary>
        protected readonly Creator creator;
        protected readonly InputStuff input;
        protected readonly DefaultPaths basePaths;
        protected readonly SaveStateManager saveStateManager;

        private readonly bool randomScreenshots;
        protected readonly Screenshotter screenshotter;
        private readonly int screenshotsRandSecondsOffset;
        private int lastRandomScreenshotMinute;

        private bool pauseUpdate;

        private Action? startRecordingPlayback;

        /// <summary>Only used for input that shouldn't be recorded (Fullscreen Toggle for example or Replay interrupt).
        /// The difference to InputStuff.KeysDev</summary>
        protected KeyInput metaKeys;

        public GamePrototype(GameWrapper g, DefaultPaths paths, ContentConverter contentConverter,
            bool mouseWithActivationClick = false, bool contentModdingOnRelease = false,
            bool vsync = true, bool startRecordingInstantly = true, bool enableDevKeys = false,
            bool randomScreenshots = false, bool clearHotReloadOutputPath = true, bool controlViaF5 = true)
            : base(g, contentModdingOnRelease, contentConverter, clearHotReloadOutputPath)
        {
            if (randomScreenshots)
            {
                this.randomScreenshots = randomScreenshots;
                screenshotsRandSecondsOffset = new Random().Next(60);
            }

            updateSpeed = new GameSpeed(g.TargetElapsedTime);
            drawSpeed = new GameSpeed(g.TargetElapsedTime);

            var converters = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Color), str => ColorExtension.FromHex(str) }
            };
            creator = new Creator("BytingLib.Markup", new[] { typeof(MarkupRoot).Assembly }, new object[] { contentCollector }, typeof(MarkupShortcutAttribute), converters);

            input = new InputStuff(mouseWithActivationClick, windowManager, g, paths, f => startRecordingPlayback = f, startRecordingInstantly, enableDevKeys, controlViaF5);

            basePaths = paths;
            saveStateManager = new SaveStateManager(paths);

            screenshotter = new Screenshotter(gDevice, paths);

            InitWindowAndGraphics(vsync);

            metaKeys = new KeyInput(Keyboard.GetState);
        }

        protected virtual void InitWindowAndGraphics(bool vsync)
        {
            if (vsync)
            {
                // enable vsync for disabling stuttering, which probably appears mostly in window mode
                graphics.SynchronizeWithVerticalRetrace = true;
                graphics.ApplyChanges();
            }
            // maximize window
            windowManager.MaximizeWindow();
        }

        public sealed override void UpdateActive(GameTime gameTime)
        {
            input.UpdateDevInput();
            metaKeys.Update();

            int iterations = GetIterations();

            for (int i = 0; i < iterations; i++)
                UpdateSingleIteration(gameTime);

            ScreenshotType screenshot = ScreenshotType.None;

            if (metaKeys.F12.Pressed)
                screenshot = ScreenshotType.ByUser;
            else if (randomScreenshots)
            {
                // take a screenshot every minute
                int currentMinute = (int)((gameTime.TotalGameTime.TotalSeconds + screenshotsRandSecondsOffset) / 60d);
                if (currentMinute > lastRandomScreenshotMinute)
                {
                    lastRandomScreenshotMinute = currentMinute;
                    screenshot = ScreenshotType.Random;
                }
            }
            if (screenshot != ScreenshotType.None)
                screenshotter.TakeScreenshot(screenshot == ScreenshotType.Random);

            if (startRecordingPlayback != null)
            {
                var copy = startRecordingPlayback;
                startRecordingPlayback = null;
                copy.Invoke();
            }
        }

        private int GetIterations()
        {
            int iterations = 1;
            if (!pauseUpdate && input.KeysDev.Alt.Down)
            {
                iterations *= 10;
                if (input.KeysDev.Apps.Down)
                    iterations *= 10;
            }
            else
            {
                if (input.KeysDev.Apps.Down)
                {
                    pauseUpdate = true;

                    if (input.KeysDev.Alt.Pressed)
                        iterations = 1; // display next frame
                    else
                        iterations = 0;
                }
                else
                {
                    pauseUpdate = false;
                }
            }
            return iterations;
        }

        private void UpdateSingleIteration(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            input.Update();

            if (metaKeys.F11.Pressed)
                windowManager.ToggleFullscreen();
            if (ShouldSwapScreen())
                windowManager.SwapScreen();

            UpdateIteration(gameTime);
        }

#if DEBUG
        protected virtual bool ShouldSwapScreen() => metaKeys.Tab.Pressed;
#else
        protected virtual bool ShouldSwapScreen() => metaKeys.Control.Down && metaKeys.Tab.Pressed;
#endif

        public sealed override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            DrawIteration(gameTime);
        }

        protected abstract void UpdateIteration(GameTime gameTime);
        protected abstract void DrawIteration(GameTime gameTime);

        public override void DrawInactiveOnce()
        {
            //#if DEBUG
            //            base.DrawInactiveOnce();
            //#endif
        }

        public override void Dispose()
        {
            screenshotter?.Dispose();

            input.Dispose();

            base.Dispose();
        }

        enum ScreenshotType
        {
            None,
            ByUser,
            Random
        }
    }
}
