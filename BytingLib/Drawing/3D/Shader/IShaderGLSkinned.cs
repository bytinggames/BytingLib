namespace BytingLib
{
    public interface IShaderGLSkinned : IShaderGL
    {
        EffectParameterStack<Matrix[]> JointMatrices { get; }
    }
}
