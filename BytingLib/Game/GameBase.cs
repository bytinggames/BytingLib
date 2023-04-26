namespace BytingLib
{
    public abstract class GameBase : DisposableContainer, IGameBase
    {
        protected readonly GameWrapper gameWrapper;
        protected readonly GraphicsDevice gDevice;
        protected readonly SpriteBatch spriteBatch;
        protected readonly WindowManager windowManager;
        protected readonly ContentManagerRawPipe contentRawPipe;
        protected readonly IContentCollector contentCollector;
        protected readonly GraphicsDeviceManager graphics;
        protected readonly Action Exit;

        public HotReloadContent? HotReloadContent { get; }

        public GameBase(GameWrapper g, bool contentModdingOnRelease, ContentConverter contentConverter)
        {
            gameWrapper = g;
            gDevice = g.GraphicsDevice;
            graphics = g.Graphics;
            g.Window.AllowUserResizing = true;
            Exit = g.Exit;

            spriteBatch = new SpriteBatch(gDevice);
            disposables.Add(spriteBatch);

            contentRawPipe = Use(new ContentManagerRawPipe(new ContentManagerRaw(g.Services, "Content")));
            contentCollector = new ContentCollector(contentRawPipe, g.GraphicsDevice);

#if DEBUG
            HotReloadContent = new HotReloadContent(g.Services,
                contentCollector,
                Path.Combine("..", "..", "..", "Content"),
                contentConverter);
            contentRawPipe.ContentManagers.Insert(0, HotReloadContent.TempContentRaw);
#else
            if (contentModdingOnRelease)
            {
                HotReloadContent = new HotReloadContent(g.Services, contentCollector, "ContentMod", contentConverter);
                contentRawPipe.ContentManagers.Insert(0, HotReloadContent.TempContentRaw);
            }
#endif


#if WINDOWS
            bool realFullscreen = false;
#else
		bool realFullscreen = true;
#endif
            windowManager = new WindowManager(realFullscreen, g.Window, g.Graphics);
        }

        public abstract void UpdateActive(GameTime gameTime);
        public virtual void UpdateInactive(GameTime gameTime) { }

        public abstract void DrawActive(GameTime gameTime);

        public virtual void DrawInactiveOnce()
        {
            // draw black blend to hint to the user, that the window isn't active
            spriteBatch.Begin();
            spriteBatch.DrawRectangle(new Rect(0, 0, gDevice.Viewport.Width, gDevice.Viewport.Height), Color.Black * 0.25f);
            spriteBatch.End();
        }

        public virtual void OnActivate()
        {
            HotReloadContent?.UpdateChanges();
        }

        public virtual void OnDeactivate()
        {
        }
    }
}
