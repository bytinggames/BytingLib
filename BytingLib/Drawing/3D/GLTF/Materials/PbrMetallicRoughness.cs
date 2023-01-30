namespace BytingLib
{
    public class PbrMetallicRoughness
    {
        public Vector4? BaseColor;
        public TextureGL? BaseColorTexture;
        public float? MetallicFactor;
        public float? RoughnessFactor;

        public IDisposable Use(IShaderPbr shader)
        {
            DisposableContainer toDispose = new();
            if (BaseColor != null)
                toDispose.UseCheckNull(shader.Color.Use(BaseColor.Value));
            if (BaseColorTexture != null)
            {
                toDispose.Use(shader.UseSampler(BaseColorTexture.Sampler.SamplerState));
                toDispose.UseCheckNull(shader.ColorTex.Use(BaseColorTexture.Image.Tex2D.Value));
            }

            if (MetallicFactor != null)
                toDispose.UseCheckNull(shader.MetallicFactor.Use(MetallicFactor.Value));
            if (RoughnessFactor != null)
                toDispose.UseCheckNull(shader.RoughnessFactor.Use(RoughnessFactor.Value));

            return toDispose;
        }
    }
}
