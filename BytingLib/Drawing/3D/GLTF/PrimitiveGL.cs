using System.Text.Json.Nodes;

namespace BytingLib
{
    public class PrimitiveGL
    {
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer? IndexBuffer { get; }
        public MaterialGL? Material { get; }

        public PrimitiveGL(ModelGL model, JsonNode n)
        {
            var attributesObj = n["attributes"]!.AsObject();

            string key = string.Concat(attributesObj.Select(f => f.Key + f.Value));

            VertexBuffer = model.GetVertexBuffer(key, attributesObj);

            JsonNode? t;
            if ((t = n["indices"]) != null)
            {
                int indicesAccessorIndex = t.GetValue<int>();
                IndexBuffer = model.GetIndexBuffer(indicesAccessorIndex);
            }

            if ((t = n["material"]) != null)
            {
                int materialId = t.GetValue<int>();
                Material = model.Materials!.Get(materialId);
            }
        }

        public void Draw(IShaderGL shader)
        {
            using (Material == null ? null : shader.UseMaterial(Material))
            {
                if (IndexBuffer == null)
                    shader.Draw(VertexBuffer);
                else
                    shader.Draw(VertexBuffer, IndexBuffer);
            }
        }
    }
}
