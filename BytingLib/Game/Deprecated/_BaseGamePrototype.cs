using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    [Obsolete]
    public abstract class _BaseGamePrototype<Ingame> : _BaseGame where Ingame : class, IStuffDisposable
    {
        protected readonly GameSpeed updateSpeed, drawSpeed;
        protected IStuffDisposable gameStuff;
        protected readonly Random rand = new Random();
        protected Ingame? ingame = null;
        protected KeyInput keys;
        protected MouseInput mouse;
        protected GamePadInput gamePad;
        protected bool triggerRestart;
        protected WindowManager windowManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public _BaseGamePrototype()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            IsMouseVisible = true;
            // factory code
            updateSpeed = new GameSpeed(TargetElapsedTime);
            drawSpeed = new GameSpeed(TargetElapsedTime);
        }

        protected override void MyInitialize()
        {
            gameStuff = _BaseGameFactory.CreateDefaultGame(this, graphics, "input", out keys, out mouse, out gamePad, out windowManager, false); disposables.Add(gameStuff);

            CreateIngame();
        }

        private void CreateIngame()
        {
            gameStuff.Add(ingame = CreateMyIngame());
        }

        protected abstract Ingame CreateMyIngame();

        protected override void UpdateActive(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            gameStuff.ForEach<IUpdate>(f => f.Update());
            if (triggerRestart)
            {
                Restart();
            }
        }

        void Restart()
        {
            if (ingame != null)
                gameStuff.Remove(ingame);
            CreateIngame();
            triggerRestart = false;

            SuppressDraw();
        }

        protected override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp);

            gameStuff.ForEach<IDraw>(f => f.Draw(spriteBatch));

            spriteBatch.End();
        }
    }
}
