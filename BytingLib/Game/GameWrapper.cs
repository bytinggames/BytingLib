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

        /// <summary>more than 16 msaaSamples is not recommended (made everything a bit pale on my system)</summary>
        public GameWrapper(Func<GameWrapper, IGameBase> createMyGame, int? msaaSamples)
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            if (msaaSamples != null && msaaSamples < 2)
                msaaSamples = null;

            if (msaaSamples != null)
            {
                Graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
                Graphics.GraphicsProfile = GraphicsProfile.HiDef;
                Graphics.PreferMultiSampling = true;
            }

            this.createMyGame = createMyGame;
            this.msaaSamples = msaaSamples;
        }

        void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
        {
            if (msaaSamples != null)
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = msaaSamples.Value;
        }

        protected override void Initialize()
        {
            var _ = new Texture2D(GraphicsDevice, 1, 1); // somehow there must be at least one texture created before a render target with multi sampling can be used...?

            base.Initialize();

            game = createMyGame(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                game?.UpdateActive(gameTime);
            }
            else
                game?.UpdateInactive(gameTime);

            if (IsActive && !previousUpdateWasActive)
            {
                game?.OnActivate();
            }
            else if (!IsActive && previousUpdateWasActive)
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

            base.Dispose(disposing);
        }

        public bool IsActivatedThisFrame()
        {
            return IsActive && !previousUpdateWasActive;
        }
    }
}
