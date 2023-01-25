namespace BytingLib
{
    public class GameWrapper : Game
    {
        private IGameBase? game;
        public readonly GraphicsDeviceManager Graphics;
        private readonly Func<GameWrapper, IGameBase> createMyGame;
        private bool previousUpdateWasActive = true;
        private bool previousDrawWasActive = true;

        public GameWrapper(Func<GameWrapper, IGameBase> createMyGame)
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            this.createMyGame = createMyGame;
        }

        protected override void Initialize()
        {
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
