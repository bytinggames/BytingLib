namespace BytingLib
{
    public interface IGameSpeed
    {
        float Factor { get; }
        GameTime GameTime { get; }
        void OnRefresh(GameTime gameTime);
    }
}