using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    /// <summary>
    /// Inherits <see cref="Game"/> and extends it with:
    /// <list>
    /// <item>Only calling derived <see cref="UpdateActive"/> and <see cref="DrawActive"/> methods when the window is active.</item>
    /// <item>A list of disposables that get disposed when the game gets disposed.</item>
    /// <item>Default Initialization (graphics, spriteBatch, rawContent, contentCollector, hotReloadContent)</item>
    /// </list>
    /// </summary>
    public abstract class BaseGame : Game
    {
        protected readonly GraphicsDeviceManager graphics;
        protected readonly List<IDisposable> disposables = new List<IDisposable>();

        /// <summary>Initialized after <see cref="LoadContent"/> has been called.</summary>
        protected SpriteBatch spriteBatch;
        /// <summary>Initialized after <see cref="LoadContent"/> has been called.</summary>
        protected IContentManagerRaw rawContent;
        /// <summary>Initialized after <see cref="LoadContent"/> has been called.</summary>
        protected IContentCollector contentCollector;
        /// <summary>Initialized after <see cref="LoadContent"/> has been called.</summary>
        protected HotReloadContent hotReloadContent;

        protected Color? InactiveBlendColor { get; set; } = Color.Black * 0.25f;

        private bool previousUpdateWasActive = true;
        private bool previousDrawWasActive = true;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public BaseGame()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected abstract void UpdateActive(GameTime gameTime);
        /// <summary>
        /// No need to call base.DrawActive() cause the base implementation only calls 
        /// <code>GraphicsDevice.Clear(Color.CornflowerBlue);</code>
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void DrawActive(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
        }
        protected abstract void MyInitialize();

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            disposables.Add(spriteBatch);

            ContentManagerRawPipe _rawContent = new ContentManagerRawPipe(new ContentManagerRaw(Services, "Content")); disposables.Add(_rawContent);
            rawContent = _rawContent;
            contentCollector = new ContentCollector(rawContent); disposables.Add(contentCollector);

#if DEBUG
            hotReloadContent = new HotReloadContent(Services, contentCollector);
#else
            hotReloadContent = new HotReloadContent(Services, contentCollector, "ContentMod");
#endif
            _rawContent.contentManagers.Insert(0, hotReloadContent.TempContentRaw);

            MyInitialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                UpdateActive(gameTime);
            }

            if (IsActive && !previousUpdateWasActive)
            {
                hotReloadContent?.UpdateChanges();
            }
            if (!IsActive)
            {
                // suppress draw, but only after the black blend has been drawn
                if (!previousDrawWasActive
                    || !InactiveBlendColor.HasValue) // or when no blend is drawn, we can suppress draw now
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
                DrawActive(gameTime);
            }
            else if (previousDrawWasActive && InactiveBlendColor.HasValue)
            {
                // draw black blend to hint to the user, that the window isn't active
                spriteBatch.Begin();

                spriteBatch.DrawRectangle(new Rect(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), InactiveBlendColor.Value);

                spriteBatch.End();
            }

            previousDrawWasActive = IsActive;

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            while (disposables.Count > 0)
            {
                disposables[^1].Dispose();
                disposables.RemoveAt(disposables.Count - 1);
            }

            base.UnloadContent();
        }
    }
}
