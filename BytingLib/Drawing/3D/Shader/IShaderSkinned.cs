namespace BytingLib
{

    public interface IShaderSkinned : IShader
    {
        /// <summary>First Half is devoted to the real joint matrices while the second half represents the inverteded transpose of those.</summary>
        EffectParameterStack<Matrix[]> JointMatrices { get; }
    }
}
