using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MaterialGL
    {
        public readonly string? Name;
        public override string ToString() => "Material: " + Name;

        public PbrMetallicRoughness? PbrMetallicRoughness;
        public RasterizerState? RasterizerState;

        public MaterialGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["pbrMetallicRoughness"]) != null)
            {
                // get texture
                var baseColorTexture = t["baseColorTexture"];
                if (baseColorTexture != null)
                {
                    int texIndex = baseColorTexture["index"]!.GetValue<int>();
                    TextureGL textureSampler = model.Textures!.Get(texIndex);

                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColorTexture = textureSampler;
                }

                // get base color
                var baseColorFactor = t["baseColorFactor"];
                if (baseColorFactor != null)
                {
                    float[] c = baseColorFactor.AsArray().Select(f => f!.GetValue<float>()).ToArray();
                    PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    PbrMetallicRoughness.BaseColor = new Vector4(c[0], c[1], c[2], c[3]);
                }
            }

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

        internal IDisposable Use(IShaderGLSkinned shader)
        {
            DisposableContainer disposables = new();
            if (PbrMetallicRoughness != null)
                disposables.Use(PbrMetallicRoughness.Use(shader));
            if (RasterizerState != null)
                disposables.Use(shader.UseRasterizer(RasterizerState));
            return disposables;
        }
    }
}
