namespace BytingLib
{
    public interface IShaderAlbedoMapEnabled : IShader
    {
        EffectParameterStack<bool> AlbedoMapEnabled { get; }
    }
}
