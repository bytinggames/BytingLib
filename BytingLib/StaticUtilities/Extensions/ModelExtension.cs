namespace BytingLib
{
    public static class ModelExtension
    {
        public static void Draw(this Model model, IShaderWorld shaderWorld, IShaderAlbedo? shaderAlbedo)
            => shaderWorld.Draw(model, shaderAlbedo);
    }
}
