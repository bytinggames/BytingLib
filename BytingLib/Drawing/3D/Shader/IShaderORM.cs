namespace BytingLib
{
    public interface IShaderORM : IShader
    {
        EffectParameterStack<Texture2D> ORMTex { get; }
    }
}
