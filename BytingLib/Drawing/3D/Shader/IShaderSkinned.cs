namespace BytingLib
{

    public interface IShaderSkinned : IShader
    {
        EffectParameterStack<Matrix[]> JointMatrices { get; }
    }
}
