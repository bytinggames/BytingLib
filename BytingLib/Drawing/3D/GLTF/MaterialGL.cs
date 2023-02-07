using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MaterialGL
    {
        public string? Name { get; set; }

        public PbrMetallicRoughness? PbrMetallicRoughness { get; set; }
        public RasterizerState? RasterizerState { get; set; }
        public TextureGL? NormalTexture { get; set; }
        public TextureGL? ORMTexture { get; set; }
        public Vector3? EmissiveFactor { get; set; }
        public TextureGL? EmissiveTexture { get; set; }

        public MaterialGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;

            #region Metallic Roughness

            if ((t = n["pbrMetallicRoughness"]) != null)
            {
                PbrMetallicRoughness = new PbrMetallicRoughness();
                // get Metallic
                var metallicFactor = t["metallicFactor"];
                if (metallicFactor != null)
                {
                    PbrMetallicRoughness.MetallicFactor = metallicFactor.GetValue<float>();
                }

                // get Roughness
                var roughnessFactor = t["roughnessFactor"];
                if (roughnessFactor != null)
                {
                    PbrMetallicRoughness.RoughnessFactor = roughnessFactor.GetValue<float>();
                }

                // get texture
                var baseColorTexture = t["baseColorTexture"];
                if (baseColorTexture != null)
                {
                    int texIndex = baseColorTexture["index"]!.GetValue<int>();
                    TextureGL textureSampler = model.Textures!.Get(texIndex)!;

                    PbrMetallicRoughness.BaseColorTexture = textureSampler;
                }

                // get base color
                var baseColorFactor = t["baseColorFactor"];
                if (baseColorFactor != null)
                {
                    float[] c = baseColorFactor.AsArray().Select(f => f!.GetValue<float>()).ToArray();
                    ColorExtension.LinearToSrgb(c);
                    PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c[3]);
                }
            }

            #endregion

            #region Emissive

            if ((t = n["emissiveFactor"]) != null)
                EmissiveFactor = t.AsArray().GetVector3();

            if ((t = n["emissiveTexture"]) != null)
            {
                int texIndex = t["index"]!.GetValue<int>();
                EmissiveTexture = model.Textures!.Get(texIndex)!;
            }

            #endregion

            #region ORM

            if ((t = n["occlusionTexture"]) != null)
            {
                int texIndex = t["index"]!.GetValue<int>();
                ORMTexture = model.Textures!.Get(texIndex)!;
            }

            #endregion

            #region Normal

            if ((t = n["normalTexture"]) != null)
            {
                int texIndex = t["index"]!.GetValue<int>();
                NormalTexture = model.Textures!.Get(texIndex)!;
            }

            #endregion


            var extras = n["extras"];
            if (extras != null)
            {
                // get extra base color
                var baseColorFactor = extras["baseColorFactor"];
                if (baseColorFactor != null)
                {
                    float[] c = baseColorFactor.AsArray().Select(f => f!.GetValue<float>()).ToArray();
                    ColorExtension.LinearToSrgb(c);
                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c.Length > 3 ? c[3] : 1f);
                }

                var culling = extras["culling"];
                if (culling != null)
                {
                    int cull = culling.GetValue<int>();
                    if (cull > 0)
                        RasterizerState = RasterizerState.CullClockwise;
                    else if (cull < 0)
                        RasterizerState = RasterizerState.CullCounterClockwise;
                    else
                        RasterizerState = RasterizerState.CullNone;
                }
            }
        }

        public override string ToString() => "Material: " + Name;
    }
}
