namespace BytingLib
{
    public interface IShaderNormalMap : IShader
    {
        EffectParameterStack<Texture2D> NormalTex { get; }
    }
}
