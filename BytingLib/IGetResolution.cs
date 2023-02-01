namespace BytingLib
{
    public interface IResolution
    {
        Int2 Resolution { get; }
        event Action<Int2>? OnResolutionChanged;
    }

    public static class IResolutionExtension
    {
        public static float GetAspectRatio(this IResolution res)
        {
            Int2 r = res.Resolution;
            return (float)r.X / r.Y;
        }
    }
}
