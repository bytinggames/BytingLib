namespace BytingLib
{
    public interface IShaderMaterial : IShader
    {
        abstract IDisposable UseMaterial(MaterialGL material);
    }
}
