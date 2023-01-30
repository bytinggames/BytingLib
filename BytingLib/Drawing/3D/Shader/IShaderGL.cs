namespace BytingLib
{
    public interface IShaderGL : IShaderTexWorld
    {
        EffectParameterStack<Vector4> Color { get; }
    }
}
