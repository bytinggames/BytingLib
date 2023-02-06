using System.Text.Json.Nodes;

namespace BytingLib
{
    public class TextureGL
    {
        public ImageGL Image;
        public SamplerGL Sampler;

        public TextureGL(ModelGL model, JsonNode n)
        {
            JsonNode? samplerNode = n["sampler"];
            if (samplerNode != null)
            {
                int samplerId = n["sampler"]!.GetValue<int>();
                Sampler = model.Samplers!.Get(samplerId)!;
            }
            else
            {
                model.DefaultSampler ??= new SamplerGL(SamplerState.LinearWrap);
                Sampler = model.DefaultSampler;
            }
            int sourceId = n["source"]!.GetValue<int>();
            Image = model.Images!.Get(sourceId)!;
        }
    }
}
