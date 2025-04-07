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
        protected readonly InputStuff inputGlobalAndDraw;
        protected readonly DefaultPaths basePaths;
        protected readonly SaveStateManager saveStateManager;
        protected readonly MouseVisibilityManager mouseVisibilityManager;
        private readonly InputControlGameSpeed? inputGameSpeed;
        private readonly InputInputRecordings? inputInputRecordings;
        protected readonly InputUpdater globalInputUpdater;
        protected readonly InputUpdater globalAndDrawInputUpdater;
        protected readonly InputUpdater metaInputUpdater;
        protected readonly BindsCanvas bindsCanvas = new();
        protected readonly BindsMeta bindsMeta = new();
        protected readonly BindsControlGameSpeed bindsControlGameSpeed = new();
        protected readonly BindsInputRecordings bindsInputRecordings = new();
        protected readonly InputCanvas inputCanvas;
        /// <summary>Only used for input that shouldn't be recorded (Fullscreen Toggle for example or Replay interrupt).
        /// The difference to inputDev</summary>
        protected readonly InputMeta inputMeta;

        private readonly bool randomScreenshots;
        protected readonly Screenshotter screenshotter;
        private readonly int screenshotsRandSecondsOffset;
        private int lastRandomScreenshotMinute;
        protected bool f11ToToggleFullscreen = true;
        protected bool uiNavigationEnabled = false;
        /// <summary>Disable to prevent any input</summary>
        protected bool updateSourceInput = true;

        private bool pauseUpdate;

        private Action? startRecordingPlayback;

        public event Action? OnFrameBeforeScreenshot;
        private int takeScreenshotNextFrame = -1;
        private DateTime? lastScreenshotTaken;

        /// <summary>Only used for easy access on frames for when debugging.</summary>
        public static Func<int> DebugGetFrame { get; set; } = () => 0;
        public static int DebugFrame => DebugGetFrame();

        public GamePrototype(GameWrapper g, DefaultPaths paths, ContentConverter contentConverter, HotReloadType hotReloadType,
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

            updateSpeed = new GameSpeed(TimeSpan.FromSeconds(g.TargetGameSpeed.Update.IntervalSeconds ?? 1d / 60d));
            drawSpeed = new GameSpeed(TimeSpan.FromSeconds(1d / 60d));

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

            input = new InputStuff(mouseWithActivationClick, windowManager, g, paths, f => startRecordingPlayback = f, startRecordingInstantly, inputInputRecordings);
            inputGlobalAndDraw = new InputStuff(mouseWithActivationClick, windowManager, g, paths, f => startRecordingPlayback = f, startRecordingInstantly, inputInputRecordings);

            globalInputUpdater = new(() => input.FullInput, "Global");
            globalAndDrawInputUpdater = new(() => inputGlobalAndDraw.FullInput, "Draw");
            metaInputUpdater = new(input.GetRealInput, "Meta");

            inputCanvas = Use(new InputCanvas(bindsCanvas, globalInputUpdater));
            inputMeta = Use(new InputMeta(bindsMeta, metaInputUpdater));

            if (enableGameSpeedKeys)
            {
                inputGameSpeed = Use(new InputControlGameSpeed(bindsControlGameSpeed, globalInputUpdater));
            }
            if (enableRecordingKeys)
            {
                inputInputRecordings = Use(new InputInputRecordings(bindsInputRecordings, globalInputUpdater));
            }

            basePaths = paths;
            saveStateManager = new SaveStateManager(paths.SaveStateDir, false);

            screenshotter = new Screenshotter(gDevice, paths);
            screenshotter.OnTakeScreenshot += Screenshotter_OnTakeScreenshot;

            InitWindowAndGraphics(vsync);

            mouseVisibilityManager = new MouseVisibilityManager(gameWrapper, windowManager, () => uiNavigationEnabled, () => inputCanvas.MousePosition.Delta != Vector2.Zero);
        }

        private void Screenshotter_OnTakeScreenshot()
        {
            lastScreenshotTaken = DateTime.UtcNow;
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
            UpdateSourceInput();

            globalInputUpdater.Update();
            globalAndDrawInputUpdater.Update();
            metaInputUpdater.Update();

            int iterations = GetIterations();

            for (int i = 0; i < iterations; i++)
            {
                UpdateSingleIteration(gameTime);
                if (i + 1 < iterations)
                {
                    globalInputUpdater.Update();
                    globalAndDrawInputUpdater.Update();
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

            UpdateSourceInput();

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

        private void UpdateSourceInput()
        {
            if (updateSourceInput)
            {
                // this updates the input queue
                input.PreUpdate();
                inputGlobalAndDraw.PreUpdate();
            }
        }

        protected virtual bool ShouldSwapScreen() => inputMeta.SwapScreen.Pressed;

        public sealed override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            if (lastScreenshotTaken.HasValue)
            {
                if ((DateTime.UtcNow - lastScreenshotTaken.Value).TotalMilliseconds > 100)
                {
                    lastScreenshotTaken = null;
                }
                // ensure that at least one frame is black
                gDevice.Clear(Color.Black);
                return;
            }

            // this updates the input queue
            if (updateSourceInput)
            {
                inputGlobalAndDraw.PreUpdate();
            }
            inputGlobalAndDraw.Update();
            globalAndDrawInputUpdater.Update();

            DrawIteration(gameTime);
        }

        protected abstract void UpdateIteration(GameTime gameTime);
        protected abstract void DrawIteration(GameTime gameTime);

        protected abstract Scene? GetTopmostScene();

        public override void DrawInactiveOnce(GameTime gameTime)
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
            inputGlobalAndDraw.Dispose();

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
