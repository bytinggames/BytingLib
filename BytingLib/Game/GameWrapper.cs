namespace BytingLib
{
    public class GameWrapper : Game, IMouseVisible
    {
        private IGameBase? game;
        public readonly GraphicsDeviceManager Graphics;
        private readonly Func<GameWrapper, IGameBase> createMyGame;
        private readonly int? msaaSamples;
        private readonly bool alwaysActive;
        private bool previousUpdateWasActive = true;
        private bool previousDrawWasActive = true;
        public bool IsExited { get; private set; }
        public TargetGameSpeed TargetGameSpeed { get; }
        private bool firstFrameClear = true;
        private int drawCounter = 0;

        /// <summary>Is set by Activated and Deactivated events. Maybe this is more precise than base.IsActive. Needs testing.</summary>
        public new bool IsActive { get; private set; }

        /// <summary>more than 16 msaaSamples is not recommended (made everything a bit pale on my system)</summary>
        public GameWrapper(Func<GameWrapper, IGameBase> createMyGame, int? msaaSamples, TargetGameSpeed targetGameSpeed, bool alwaysActive = false)
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            if (msaaSamples != null && msaaSamples < 2)
            {
                msaaSamples = null;
            }

            if (msaaSamples != null)
            {
                Graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
                Graphics.GraphicsProfile = GraphicsProfile.HiDef;
                Graphics.PreferMultiSampling = true;
            }

            this.createMyGame = createMyGame;
            this.msaaSamples = msaaSamples;
            this.TargetGameSpeed = targetGameSpeed;
            this.alwaysActive = alwaysActive;
            targetGameSpeed.SetTargetElapsedSeconds += SetTargetElapsedSeconds;
            Activated += GameWrapper_Activated;
            Deactivated += GameWrapper_Deactivated;

            if (alwaysActive)
            {
                IsActive = true;
            }
        }

        private void SetTargetElapsedSeconds(double frameTime)
        {
            TargetElapsedTime = TimeSpan.FromSeconds(frameTime);
        }

        private void GameWrapper_Activated(object? sender, EventArgs e)
        {
            IsActive = true;
        }

        private void GameWrapper_Deactivated(object? sender, EventArgs e)
        {
            if (!alwaysActive)
            {
                IsActive = false;
            }
        }

        private void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
        {
            if (msaaSamples != null)
            {
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = msaaSamples.Value;
            }
        }

        protected override void Initialize()
        {
            var _ = new Texture2D(GraphicsDevice, 1, 1); // somehow there must be at least one texture created before a render target with multi sampling can be used...?

            base.Initialize();

            game = createMyGame(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive && !previousUpdateWasActive)
            {
                game?.OnActivate();
            }

            if (!TargetGameSpeed.Update.ShouldSkip(TargetElapsedTime, IsFixedTimeStep))
            {
                if (IsActive)
                {
                    game?.UpdateActive(TargetGameSpeed.Update.GameTime);
                }
                else
                {
                    game?.UpdateInactive(TargetGameSpeed.Update.GameTime);
                }
            }
            else
            {

            }

            if (!IsActive && previousUpdateWasActive)
            {
                game?.OnDeactivate();
            }
            if (!IsActive)
            {
                // suppress draw, but only after the black blend has been drawn
                if (!previousDrawWasActive)
                {
                    SuppressDraw();
                }
            }
            previousUpdateWasActive = IsActive;

            base.Update(gameTime);
        }

        protected override bool BeginDraw()
        {
            drawCounter++;
            
            if (
                // once every 60 ticks let at least draw once, so we at least have 1 fps
                // if update is running slow, skip draws
                drawCounter % 60 != 0 && TargetGameSpeed.Update.IsRunningSlow()
                || TargetGameSpeed.Draw.ShouldSkip(TargetElapsedTime, IsFixedTimeStep)
                )
            {
                return false;
            }
            return base.BeginDraw();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (firstFrameClear)
            {
                GraphicsDevice.Clear(Color.Black);
                firstFrameClear = false;
            }

            if (IsActive)
            {
                game?.DrawActive(TargetGameSpeed.Draw.GameTime);
            }
            else if (previousDrawWasActive)
            {
                game?.DrawInactiveOnce(TargetGameSpeed.Draw.GameTime);
            }

            previousDrawWasActive = IsActive;

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            TargetGameSpeed.SetTargetElapsedSeconds -= SetTargetElapsedSeconds;
            game?.Dispose();
            game = null;

            Activated -= GameWrapper_Activated;
            Deactivated -= GameWrapper_Deactivated;

            base.Dispose(disposing);
        }

        public bool IsActivatedThisFrame()
        {
            return IsActive && !previousUpdateWasActive;
        }

        public new void Exit()
        {
            base.Exit();

            IsExited = true;
        }
    }
}
