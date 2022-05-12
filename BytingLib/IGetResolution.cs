namespace BytingLib
{
    public interface IGetResolution
    {
        Int2 GetResolution();
        event Action<Int2>? OnResolutionChanged;
    }

}
