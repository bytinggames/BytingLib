namespace BytingLib
{
    public static class IGameSpeedExtension
    {
        public static double TotalMS(this IGameSpeed gameSpeed)
        {
            return gameSpeed.GameTime.TotalGameTime.TotalMilliseconds;
        }
        public static double TotalSeconds(this IGameSpeed gameSpeed)
        {
            return gameSpeed.GameTime.TotalGameTime.TotalSeconds;
        }

        public static float TotalMSF(this IGameSpeed gameSpeed)
        {
            return (float)gameSpeed.GameTime.TotalGameTime.TotalMilliseconds;
        }
        public static float TotalSecondsF(this IGameSpeed gameSpeed)
        {
            return (float)gameSpeed.GameTime.TotalGameTime.TotalSeconds;
        }
    }
}