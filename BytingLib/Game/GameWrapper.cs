namespace BytingLib
{
    public class GameWrapper : Game, IMouseVisible
    {
        private IGameBase? game;
        public readonly GraphicsDeviceManager Graphics;
        private readonly Func<GameWrapper, IGameBase> createMyGame;
        private readonly int? msaaSamples;
        private bool previousUpdateWasActive = true;
        private bool previousDrawWasActive = true;

        /// <summary>Is set by Activated and Deactivated events. Maybe this is more precise than base.IsActive. Needs testing.</summary>
        public new bool IsActive { get; private set; }

        /// <summary>more than 16 msaaSamples is not recommended (made everything a bit pale on my system)</summary>
        public GameWrapper(Func<GameWrapper, IGameBase> createMyGame, int? msaaSamples)
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

            Activated += GameWrapper_Activated;
            Deactivated += GameWrapper_Deactivated;
        }

        private void GameWrapper_Activated(object? sender, EventArgs e)
        {
            IsActive = true;
        }

        private void GameWrapper_Deactivated(object? sender, EventArgs e)
        {
            IsActive = false;
        }

        void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
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

            if (IsActive)
            {
                game?.UpdateActive(gameTime);
            }
            else
            {
                game?.UpdateInactive(gameTime);
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

        protected override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                game?.DrawActive(gameTime);
            }
            else if (previousDrawWasActive)
            {
                game?.DrawInactiveOnce();
            }

            previousDrawWasActive = IsActive;

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
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
    }
}
