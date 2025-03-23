namespace BytingLib
{
    public class GameSpeed : IGameSpeed
    {
        public GameTime GameTime { get; private set; } = new GameTime();

        public double TargetMSPerTick { get; }

        public float Factor => (float)(GameTime.ElapsedGameTime.TotalMilliseconds / TargetMSPerTick);
        public float DeltaMS => (float)GameTime.ElapsedGameTime.TotalMilliseconds;
        public float DeltaSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;

        public GameSpeed(TimeSpan defaultElapsedTime)
        {
            TargetMSPerTick = defaultElapsedTime.TotalMilliseconds;
            if (TargetMSPerTick <= 0)
            {
                throw new ArgumentException("defaultElapsedTime must be larger than 0");
            }
        }

        public void OnRefresh(GameTime gameTime)
        {
            GameTime = gameTime;
        }
    }
}
