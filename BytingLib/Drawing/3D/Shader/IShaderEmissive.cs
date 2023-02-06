namespace BytingLib
{
    public interface IShaderEmissive : IShader
    {
        EffectParameterStack<Vector3> EmissiveFactor { get; }
        EffectParameterStack<Texture2D> EmissiveTex { get; }
    }
}
