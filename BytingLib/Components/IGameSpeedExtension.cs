namespace BytingLib
{
    public static class IGameSpeedExtension
    {
        public static double TotalMS(this IGameSpeed gameSpeed)
        {
            return gameSpeed.GameTime.TotalGameTime.TotalMilliseconds;
        }
    }
}