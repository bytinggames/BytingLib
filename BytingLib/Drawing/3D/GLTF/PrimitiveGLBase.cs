using System.Text.Json.Nodes;

namespace BytingLib
{
    public class PrimitiveGLBase
    {
        public MaterialGL? Material { get; }

        public PrimitiveGLBase(ModelGL model, JsonNode n)
        {
            JsonNode? t;
            if ((t = n["material"]) != null)
            {
                int materialId = t.GetValue<int>();
                Material = model.Materials!.Get(materialId);
            }
        }

        public PrimitiveGLBase(MaterialGL? material)
        {
            Material = material;
        }
    }
}
