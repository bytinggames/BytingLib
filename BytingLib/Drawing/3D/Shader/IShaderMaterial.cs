namespace BytingLib
{
    public interface IShaderMaterial : IShader, IShaderAlbedo
    {
        abstract void UseMaterial(DisposableContainer disposables, MaterialGL material);
    }
}
