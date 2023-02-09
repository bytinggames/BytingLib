namespace BytingLib
{
    public interface IShaderMaterial : IShader, IShaderAlbedo
    {
        abstract IDisposable UseMaterial(MaterialGL material);
    }
}
