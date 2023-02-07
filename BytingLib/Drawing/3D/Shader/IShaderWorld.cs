namespace BytingLib
{
    public interface IShaderWorld : IShader
    {
        EffectParameterStack<Matrix> World { get; }
    }
}
