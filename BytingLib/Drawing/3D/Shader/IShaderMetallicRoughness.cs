namespace BytingLib
{
    public interface IShaderMetallicRoughness
    {
        EffectParameterStack<float> MetallicFactor { get; }
        EffectParameterStack<float> RoughnessFactor { get; }
    }
}
