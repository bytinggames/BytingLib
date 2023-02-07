namespace BytingLib
{
    public interface IShaderGL : IShaderWorld, IShaderSkinned
    {
        abstract IDisposable UseMaterial(MaterialGL material);
    }
}
