using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class UpdateSpeed : IUpdateSpeed
    {
        public GameTime GameTime { get; private set; } = new GameTime();

        private readonly double defaultMSPerFrame;

        public float Factor => (float)(GameTime.ElapsedGameTime.TotalMilliseconds / defaultMSPerFrame);

        public UpdateSpeed(TimeSpan defaultElapsedTime)
        {
            defaultMSPerFrame = defaultElapsedTime.TotalMilliseconds;
            if (defaultMSPerFrame <= 0)
                throw new ArgumentException("defaultElapsedTime must be larger than 0");
        }

        public void Update(GameTime gameTime)
        {
            GameTime = gameTime;
        }
    }
}
