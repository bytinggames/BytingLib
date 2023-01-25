using BytingLib.Creation;
using BytingLib.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GameBase : DisposableContainer, IGameBase
    {
        protected readonly GraphicsDevice graphicsDevice;
        protected readonly SpriteBatch spriteBatch;
        protected readonly HotReloadContent hotReloadContent;
        protected readonly WindowManager windowManager;
        protected readonly ContentManagerRawPipe contentRawPipe;
        protected readonly IContentCollector contentCollector;
        protected readonly GraphicsDeviceManager graphics;

        protected readonly Action Exit;

        public GameBase(GameWrapper g)
        {
            graphicsDevice = g.GraphicsDevice;
            graphics = g.Graphics;
            g.Window.AllowUserResizing = true;
            Exit = g.Exit;

            spriteBatch = new SpriteBatch(graphicsDevice);
            disposables.Add(spriteBatch);

            contentRawPipe = Use(new ContentManagerRawPipe(new ContentManagerRaw(g.Services, "Content")));
            contentCollector = new ContentCollector(contentRawPipe);

#if DEBUG
            hotReloadContent = new HotReloadContent(g.Services,
                contentCollector,
                Path.Combine("..", "..", "..", "Content"));
            contentRawPipe.ContentManagers.Insert(0, hotReloadContent.TempContentRaw);
#else
            hotReloadContent = new HotReloadContent(g.Services, contentCollector, "ContentMod");
            contentRawPipe.ContentManagers.Insert(0, hotReloadContent.TempContentRaw);
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
            spriteBatch.DrawRectangle(new Rect(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black * 0.25f);
            spriteBatch.End();
        }

        public virtual void OnActivate()
        {
            hotReloadContent?.UpdateChanges();
        }

        public virtual void OnDeactivate()
        {
        }
    }
}
