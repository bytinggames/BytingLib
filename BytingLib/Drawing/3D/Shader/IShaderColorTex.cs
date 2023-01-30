namespace BytingLib
{
    public interface IShaderColorTex : IShader
    {
        EffectParameterStack<Texture2D> ColorTex { get; }
    }
}
