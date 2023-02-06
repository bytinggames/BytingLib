using System.Text.Json.Nodes;

namespace BytingLib
{
    public class TextureGL
    {
        public ImageGL Image;
        public SamplerGL Sampler;

        public TextureGL(ModelGL model, JsonNode n)
        {
            int samplerId = n["sampler"]!.GetValue<int>();
            Sampler = model.Samplers!.Get(samplerId)!;
            int sourceId = n["source"]!.GetValue<int>();
            Image = model.Images!.Get(sourceId)!;
        }
    }
}
