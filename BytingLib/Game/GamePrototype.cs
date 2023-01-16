using BytingLib.Creation;
using BytingLib.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GamePrototype : GameBase
    {
        protected readonly GameSpeed updateSpeed, drawSpeed;
        protected readonly KeyInput keys;
        protected readonly MouseInput mouse;
        protected readonly Creator creator;

        public GamePrototype(GameWrapper g) : base(g)
        {
            updateSpeed = new GameSpeed(g.TargetElapsedTime);
            drawSpeed = new GameSpeed(g.TargetElapsedTime);

            var converters = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Color), str => ColorExtension.HexToColor(str) }
            };
            creator = new Creator("BytingLib.Markup", new[] { typeof(MarkupRoot).Assembly }, null, typeof(MarkupShortcutAttribute), converters);

            keys = new KeyInput(Keyboard.GetState);
            mouse = new MouseInput(Mouse.GetState, g.IsActivatedThisFrame);

            InitWindowAndGraphics();
        }

        protected virtual void InitWindowAndGraphics()
        {
            // enable vsync for disabling stuttering, which probably appears mostly in window mode
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            // maximize window
            windowManager.MaximizeWindow();
        }

        public override void UpdateActive(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            keys.Update();
            mouse.Update();

            if (keys.F11.Pressed)
                windowManager.ToggleFullscreen();
            if (keys.Tab.Pressed)
                windowManager.SwapScreen();
        }

        public override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);
        }

        public override void DrawInactiveOnce()
        {
        }
    }
}
