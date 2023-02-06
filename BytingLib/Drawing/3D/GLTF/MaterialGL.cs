using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MaterialGL
    {
        public readonly string? Name;
        public override string ToString() => "Material: " + Name;

        public PbrMetallicRoughness? PbrMetallicRoughness;
        public RasterizerState? RasterizerState;
        public TextureGL? NormalTexture;
        public TextureGL? ORMTexture;
        public Vector3? EmissiveFactor;
        public TextureGL? EmissiveTexture;

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

        internal IDisposable Use(IShaderDefault shader, ref string techniqueName)
        {
            DisposableContainer disposables = new();
            if (PbrMetallicRoughness != null)
                disposables.Use(PbrMetallicRoughness.Use(shader));
            if (RasterizerState != null)
                disposables.Use(shader.UseRasterizer(RasterizerState));

            if (NormalTexture != null)
            {
                techniqueName += "NMap";

                disposables.Use(shader.UseSampler(NormalTexture.Sampler.SamplerState, 1));
                disposables.UseCheckNull(shader.NormalTex.Use(NormalTexture.Image.Tex2D.Value));
            }

            if (ORMTexture != null)
            {
                techniqueName += "ORM";

                disposables.Use(shader.UseSampler(ORMTexture.Sampler.SamplerState, 2));
                disposables.UseCheckNull(shader.ORMTex.Use(ORMTexture.Image.Tex2D.Value));
            }

            if (EmissiveFactor != null)
            {
                techniqueName += "Emissive";

                disposables.UseCheckNull(shader.EmissiveFactor.Use(EmissiveFactor.Value));
                if (EmissiveTexture != null)
                {
                    disposables.Use(shader.UseSampler(EmissiveTexture.Sampler.SamplerState, 3));
                    disposables.UseCheckNull(shader.EmissiveTex.Use(EmissiveTexture.Image.Tex2D.Value));
                }
                else
                    throw new NotImplementedException();
            }

            return disposables;
        }
    }
}
