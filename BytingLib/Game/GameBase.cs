using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public abstract class GameBase : DisposableContainer, IGameBase
    {
        protected readonly GameWrapper gameWrapper;
        private readonly ContentConverter contentConverter;
        private readonly string additionalContentHeader;
        protected readonly GraphicsDevice gDevice;
        protected readonly SpriteBatch spriteBatch;
        protected readonly WindowManager windowManager;
        protected readonly ContentManagerRawPipe contentRawPipe;
        protected readonly IContentCollector contentCollector;
        protected readonly GraphicsDeviceManager graphics;
        protected Action Exit;

        public HotReloadContent? HotReloadContent { get; private set; }

        public GameBase(GameWrapper g, HotReloadType hotReloadType, ContentConverter contentConverter, bool clearHotReloadOutputPath = true, string additionalContentHeader = "")
        {
            gameWrapper = g;
            gDevice = g.GraphicsDevice;
            graphics = g.Graphics;
            g.Window.AllowUserResizing = true;
            Exit = g.Exit;
            this.contentConverter = contentConverter;
            this.additionalContentHeader = additionalContentHeader;

            spriteBatch = new SpriteBatch(gDevice);
            disposables.Add(spriteBatch);

            contentRawPipe = Use(new ContentManagerRawPipe(new ContentManagerRaw(g.Services, "Content")));
            contentCollector = new ContentCollector(contentRawPipe, g.GraphicsDevice);


            switch (hotReloadType)
            {
                case HotReloadType.Modding:
                    InitializeModdingHotReloadContent();
                    break;
                case HotReloadType.Debug:
                    HotReloadContent = new HotReloadContent(g.Services,
                        contentCollector,
                        Path.Combine("..", "..", "..", "Content"),
                        contentConverter,
                        clearHotReloadOutputPath,
                        additionalContentHeader);
                    contentRawPipe.ContentManagers.Insert(0, HotReloadContent.TempContentRaw);
                    break;
            }


#if WINDOWS
            bool realFullscreen = false;
#else
		bool realFullscreen = true;
#endif
            windowManager = new WindowManager(realFullscreen, g.Window, g.Graphics);
        }

        [MemberNotNull(nameof(HotReloadContent))]
        protected void InitializeModdingHotReloadContent()
        {
            if (HotReloadContent == null)
            {
                HotReloadContent = new HotReloadContent(gameWrapper.Services, contentCollector, "ContentMod", contentConverter, true, additionalContentHeader);
                contentRawPipe.ContentManagers.Insert(0, HotReloadContent.TempContentRaw);
            }
        }

        public abstract void UpdateActive(GameTime gameTime);
        public virtual void UpdateInactive(GameTime gameTime) { }

        public abstract void DrawActive(GameTime gameTime);

        public virtual void DrawInactiveOnce(GameTime gameTime)
        {
            // draw black blend to hint to the user, that the window isn't active
            spriteBatch.Begin();
            spriteBatch.DrawRectangle(new Rect(0, 0, windowManager.ResolutionX, windowManager.ResolutionY), Color.Black * 0.25f);
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
