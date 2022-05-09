using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class GameSpeed : IGameSpeed
    {
        public GameTime GameTime { get; private set; } = new GameTime();

        private readonly double defaultMSPerFrame;

        public float Factor => (float)(GameTime.ElapsedGameTime.TotalMilliseconds / defaultMSPerFrame);

        public GameSpeed(TimeSpan defaultElapsedTime)
        {
            defaultMSPerFrame = defaultElapsedTime.TotalMilliseconds;
            if (defaultMSPerFrame <= 0)
                throw new ArgumentException("defaultElapsedTime must be larger than 0");
        }

        public void OnRefresh(GameTime gameTime)
        {
            GameTime = gameTime;
        }
    }
}
