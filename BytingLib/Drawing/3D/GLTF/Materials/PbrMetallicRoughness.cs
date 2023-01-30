namespace BytingLib
{
    public class PbrMetallicRoughness
    {
        public Vector4? BaseColor;
        public TextureGL? BaseColorTexture;
        public float MetallicFactor;
        public float RoughnessFactor;

        public IDisposable Use(IShaderGL shader)
        {
            DisposableContainer toDispose = new();
            if (BaseColor != null)
                toDispose.UseCheckNull(shader.Color.Use(BaseColor.Value));
            if (BaseColorTexture != null)
            {
                toDispose.Use(shader.UseSampler(BaseColorTexture.Sampler.SamplerState));
                toDispose.UseCheckNull(shader.ColorTex.Use(BaseColorTexture.Image.Tex2D.Value));
            }
            return toDispose;
        }
    }
}
