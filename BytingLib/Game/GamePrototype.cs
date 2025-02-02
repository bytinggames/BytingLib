using BytingLib.Markup;
using BytingLib.Serialization;

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
        protected readonly MouseVisibilityManager mouseVisibilityManager;
        private readonly InputControlGameSpeed? inputGameSpeed;
        private readonly InputRecordingBinds? inputRecordingBinds;
        protected readonly InputUpdater globalInputUpdater;
        protected readonly InputCanvas inputCanvas;
        /// <summary>Only used for input that shouldn't be recorded (Fullscreen Toggle for example or Replay interrupt).
        /// The difference to inputDev</summary>
        protected readonly InputMeta inputMeta;

        private readonly bool randomScreenshots;
        protected readonly Screenshotter screenshotter;
        private readonly int screenshotsRandSecondsOffset;
        private int lastRandomScreenshotMinute;
        protected bool f11ToToggleFullscreen = true;

        private bool pauseUpdate;

        private Action? startRecordingPlayback;

        public event Action? OnFrameBeforeScreenshot;
        private int takeScreenshotNextFrame = -1;

        /// <summary>Only used for easy access on frames for when debugging.</summary>
        public static Func<int> DebugGetFrame { get; set; } = () => 0;
        public static int DebugFrame => DebugGetFrame();

        public GamePrototype(GameWrapper g, DefaultPaths paths, ContentConverter contentConverter, HotReloadType hotReloadType, Action<Exception> onInputException,
            bool mouseWithActivationClick = false,
            bool vsync = true, bool startRecordingInstantly = true, bool enableGameSpeedKeys = false,
            bool randomScreenshots = false, bool clearHotReloadOutputPath = true, bool enableRecordingKeys = true)
            : base(g, hotReloadType, contentConverter, clearHotReloadOutputPath)
        {
            MainThread.Initialize(); // tell the main thread which thread actually is the main thread

            if (randomScreenshots)
            {
                this.randomScreenshots = randomScreenshots;
                screenshotsRandSecondsOffset = new Random().Next(60);
            }

            updateSpeed = new GameSpeed(g.TargetElapsedTime);
            drawSpeed = new GameSpeed(g.TargetElapsedTime);

            var converters = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Color), str => ColorExtension.FromHex(str) },
                { typeof(Rectangle?), str =>
                    {
                        string[] split = str.Split(new char[]{'|' });
                        if (split.Length != 4)
                        {
                            throw new Exception("rectangle arguments must be 4");
                        }

                        return new Rectangle(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
                    }
                }
            };
            creator = new Creator("BytingLib.Markup", new[] { typeof(MarkupRoot).Assembly }, new object[] { contentCollector }, typeof(MarkupShortcutAttribute), converters);

            input = new InputStuff(mouseWithActivationClick, windowManager, g, paths, f => startRecordingPlayback = f, startRecordingInstantly, inputRecordingBinds);

            globalInputUpdater = new(() => input.FullInput, onInputException);

            inputCanvas = Use(new InputCanvas(globalInputUpdater));
            inputMeta = Use(new InputMeta(globalInputUpdater));

            if (enableGameSpeedKeys)
            {
                inputGameSpeed = Use(new InputControlGameSpeed(globalInputUpdater));
            }
            if (enableRecordingKeys)
            {
                inputRecordingBinds = Use(new InputRecordingBinds(globalInputUpdater));
            }

            basePaths = paths;
            saveStateManager = new SaveStateManager(paths.SaveStateDir);

            screenshotter = new Screenshotter(gDevice, paths);

            InitWindowAndGraphics(vsync);

            mouseVisibilityManager = new MouseVisibilityManager(gameWrapper);
        }

        protected virtual void InitWindowAndGraphics(bool vsync)
        {
            if (vsync != graphics.SynchronizeWithVerticalRetrace)
            {
                graphics.SynchronizeWithVerticalRetrace = vsync;
                graphics.ApplyChanges();
            }

            SetupWindow();
        }

        protected virtual void SetupWindow()
        {
            // maximize window
            windowManager.MaximizeWindow();
        }

        public sealed override void UpdateActive(GameTime gameTime)
        {
            globalInputUpdater.Update();
            input.PreUpdate();

            int iterations = GetIterations();

            for (int i = 0; i < iterations; i++)
            {
                UpdateSingleIteration(gameTime);
                if (i + 1 < iterations)
                {
                    globalInputUpdater.Update();
                }
            }

            ScreenshotType screenshot = ScreenshotType.None;

            if (inputMeta.Screenshot.Pressed)
            {
                OnFrameBeforeScreenshot?.Invoke();
                takeScreenshotNextFrame = inputMeta.ScreenshotDelayed.Down ? 5 : 1;
            }
            else if (takeScreenshotNextFrame != -1)
            {
                takeScreenshotNextFrame--;
                if (takeScreenshotNextFrame == 0)
                {
                    takeScreenshotNextFrame = -1;
                    screenshot = ScreenshotType.ByUser;
                }
            }
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
            {
                screenshotter.TakeScreenshot(screenshot == ScreenshotType.Random);
            }

            if (startRecordingPlayback != null)
            {
                var copy = startRecordingPlayback;
                startRecordingPlayback = null;
                copy.Invoke();
            }

            double targetMS = gameWrapper.IsFixedTimeStep ? gameWrapper.TargetElapsedTime.TotalMilliseconds - 1 : 15;
            MainThread.ExecuteActions((int)targetMS);
        }

        private int GetIterations()
        {
            int iterations = 1;
            if (inputGameSpeed != null)
            {
                if (!pauseUpdate && inputGameSpeed.SpeedUp100.Down)
                {
                    iterations *= 100;
                }
                else if (!pauseUpdate && inputGameSpeed.SpeedUp10.Down)
                {
                    iterations *= 10;
                }
                else
                {
                    if (inputGameSpeed.Halt.Down)
                    {
                        pauseUpdate = true;

                        if (inputGameSpeed.ForwardOneFrame.Pressed)
                        {
                            iterations = 1; // display next frame
                        }
                        else
                        {
                            iterations = 0;
                        }
                    }
                    else
                    {
                        pauseUpdate = false;
                    }
                }
            }
            return iterations;
        }

        private void UpdateSingleIteration(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            input.Update();

            if (f11ToToggleFullscreen && inputMeta.ToggleFullscreen.Pressed)
            {
                windowManager.ToggleFullscreen();
            }

            if (ShouldSwapScreen())
            {
                windowManager.SwapScreen();
            }

            UpdateIteration(gameTime);

            mouseVisibilityManager.UpdateEnd(GetTopmostScene());
        }

        protected virtual bool ShouldSwapScreen() => inputMeta.SwapScreen.Pressed;

        public sealed override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            DrawIteration(gameTime);
        }

        protected abstract void UpdateIteration(GameTime gameTime);
        protected abstract void DrawIteration(GameTime gameTime);

        protected abstract Scene? GetTopmostScene();

        public override void DrawInactiveOnce()
        {
            //#if DEBUG
            //            base.DrawInactiveOnce();
            //#endif
        }

        public override void Dispose()
        {
            mouseVisibilityManager.Dispose();

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
