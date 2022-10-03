using BytingLib.Creation;
using BytingLib.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GameBase : DisposableContainer, IGameBase
    {
        protected readonly GraphicsDevice gDevice;
        protected readonly SpriteBatch spriteBatch;
        protected readonly HotReloadContent hotReloadContent;
        protected readonly WindowManager windowManager;
        protected readonly ContentManagerRawPipe contentRawPipe;
        protected readonly IContentCollector contentCollector;

        public GameBase(GameWrapper g, GraphicsDeviceManager graphics)
        {
            gDevice = g.GraphicsDevice;
            g.Window.AllowUserResizing = true;

            spriteBatch = new SpriteBatch(gDevice);
            disposables.Add(spriteBatch);

            contentRawPipe = Use(new ContentManagerRawPipe(new ContentManagerRaw(g.Services, "Content")));
            contentCollector = new ContentCollector(contentRawPipe);

#if DEBUG
            hotReloadContent = new HotReloadContent(g.Services, contentCollector, @"..\..\..\..\..");
            contentRawPipe.ContentManagers.Insert(0, hotReloadContent.TempContentRaw);
#else
            //hotReloadContent = new HotReloadContent(g.Services, contentCollector, "ContentMod");
            //contentRawPipe.ContentManagers.Insert(0, hotReloadContent.TempContentRaw);
#endif


            windowManager = new WindowManager(false, g.Window, graphics);
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
            hotReloadContent?.UpdateChanges();
        }

        public virtual void OnDeactivate()
        {
        }
    }
}
