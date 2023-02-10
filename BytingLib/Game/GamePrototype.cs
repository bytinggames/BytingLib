using BytingLib.Creation;
using BytingLib.Markup;

namespace BytingLib
{
    public abstract class GamePrototype : GameBase
    {
        protected readonly GameSpeed updateSpeed, drawSpeed;
        protected readonly Creator creator;

        protected readonly InputStuff input;

        public GamePrototype(GameWrapper g, bool mouseWithActivationClick = false) : base(g)
        {
            updateSpeed = new GameSpeed(g.TargetElapsedTime);
            drawSpeed = new GameSpeed(g.TargetElapsedTime);

            var converters = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Color), str => ColorExtension.HexToColor(str) }
            };
            creator = new Creator("BytingLib.Markup", new[] { typeof(MarkupRoot).Assembly }, null, typeof(MarkupShortcutAttribute), converters);

            input = new InputStuff(mouseWithActivationClick, windowManager, g);

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

        public sealed override void UpdateActive(GameTime gameTime)
        {
            input.UpdateKeysDev();

            int iterations = GetIterations();

            for (int i = 0; i < iterations; i++)
                UpdateSingleIteration(gameTime);
        }

        bool pauseUpdate;
        private int GetIterations()
        {
            int iterations = 1;
            if (!pauseUpdate && input.KeysDev.Alt.Down)
            {
                iterations *= 10;
                if (input.KeysDev.Control.Down)
                    iterations *= 10;
            }
            else
            {
                if (input.KeysDev.Control.Down)
                {
                    pauseUpdate = true;

                    if (input.KeysDev.Alt.Pressed)
                        iterations = 1; // display next frame
                    else
                        iterations = 0;
                }
                else
                {
                    pauseUpdate = false;
                }
            }
            return iterations;
        }

        private void UpdateSingleIteration(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            input.Update();

            if (input.Keys.F11.Pressed)
                windowManager.ToggleFullscreen();
            if (input.Keys.Tab.Pressed)
                windowManager.SwapScreen();

            UpdateIteration(gameTime);
        }


        public sealed override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            DrawIteration(gameTime);
        }

        protected abstract void UpdateIteration(GameTime gameTime);
        protected abstract void DrawIteration(GameTime gameTime);

        public override void DrawInactiveOnce()
        {
        }

        public override void Dispose()
        {
            input.Dispose();

            base.Dispose();
        }
    }
}
