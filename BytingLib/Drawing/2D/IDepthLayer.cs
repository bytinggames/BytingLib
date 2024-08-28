
namespace BytingLib
{
    public interface IDepthLayer
    {
        float GetDepth();
        IDisposable Use();
    }
}
