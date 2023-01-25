namespace BytingLib
{
    public interface IShaderTexWorld : IShader
    {
        EffectParameterStack<Matrix> World { get; }
        EffectParameterStack<Texture2D> ColorTex { get; }
    }
}
