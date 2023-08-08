namespace BytingLib
{
    public interface IShaderColor : IShader
    {
        EffectParameterStack<Vector4> Color { get; }
    }
}
