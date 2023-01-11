using BytingLib.Creation;
using BytingLib.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GamePrototype : GameBase
    {
        public readonly GameSpeed UpdateSpeed, DrawSpeed;
        public readonly KeyInput Keys;
        public readonly MouseInput Mouse;
        public readonly Creator Creator;

        public GamePrototype(GameWrapper g) : base(g)
        {
            UpdateSpeed = new GameSpeed(g.TargetElapsedTime);
            DrawSpeed = new GameSpeed(g.TargetElapsedTime);

            var converters = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Color), str => ColorExtension.HexToColor(str) }
            };
            Creator = new Creator("BytingLib.Markup", new[] { typeof(MarkupRoot).Assembly }, null, typeof(MarkupShortcutAttribute), converters);

            Keys = new KeyInput(Keyboard.GetState);
            Mouse = new MouseInput(Microsoft.Xna.Framework.Input.Mouse.GetState, g.IsActivatedThisFrame);

            InitWindowAndGraphics();
        }

        protected virtual void InitWindowAndGraphics()
        {
            // enable vsync for disabling stuttering, which probably appears mostly in window mode
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            // maximize window
            WindowManager.MaximizeWindow();
        }

        public override void UpdateActive(GameTime gameTime)
        {
            UpdateSpeed.OnRefresh(gameTime);

            Keys.Update();
            Mouse.Update();

            if (Keys.F11.Pressed)
                WindowManager.ToggleFullscreen();
            if (Keys.Tab.Pressed)
                WindowManager.SwapScreen();
        }

        public override void DrawActive(GameTime gameTime)
        {
            DrawSpeed.OnRefresh(gameTime);
        }

        public override void DrawInactiveOnce()
        {
        }
    }
}
