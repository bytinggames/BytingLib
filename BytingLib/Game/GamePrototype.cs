using BytingLib.Creation;
using BytingLib.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public abstract class GamePrototype : GameBase
    {
        protected readonly GameSpeed updateSpeed, drawSpeed;
        protected readonly IStuffDisposable stuff;
        protected readonly KeyInput keys;
        protected readonly MouseInput mouse;
        protected readonly Creator creator;

        public GamePrototype(GameWrapper g, GraphicsDeviceManager graphics) : base(g, graphics)
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

            stuff = Use(new StuffDisposable(typeof(IUpdate), typeof(IDraw)));
            stuff.AddRange(
                keys,
                mouse
                );
        }

        public override void UpdateActive(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            if (keys.F11.Pressed)
                windowManager.ToggleFullscreen();
            if (keys.Tab.Pressed)
                windowManager.SwapScreen();

            stuff.ForEach<IUpdate>(f => f.Update());
        }

        public override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            gDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            stuff.ForEach<IDraw>(f => f.Draw(spriteBatch));
            spriteBatch.End();
        }
    }
}
