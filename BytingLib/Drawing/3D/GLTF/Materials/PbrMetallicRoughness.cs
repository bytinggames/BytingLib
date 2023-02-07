namespace BytingLib
{
    public class PbrMetallicRoughness
    {
        public Vector4? BaseColor { get; set; }
        public TextureGL? BaseColorTexture { get; set; }
        public float? MetallicFactor { get; set; }
        public float? RoughnessFactor { get; set; }
    }
}
