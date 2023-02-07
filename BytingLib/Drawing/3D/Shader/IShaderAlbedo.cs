namespace BytingLib
{
    public interface IShaderAlbedo : IShader
    {
        EffectParameterStack<Texture2D> AlbedoTex { get; }
    }
}
