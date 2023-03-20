using System.Text.Json.Nodes;

namespace BytingLib
{
    public class PrimitiveGL : PrimitiveGLBase
    {
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer? IndexBuffer { get; }

        public PrimitiveGL(ModelGL model, JsonNode n)
            : base(model, n)
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
        }

        public void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial)
        {
            using (Material == null ? null : shaderMaterial?.UseMaterial(Material))
            {
                if (IndexBuffer == null)
                    shader.Draw(VertexBuffer);
                else
                    shader.Draw(VertexBuffer, IndexBuffer);
            }
        }
    }
}
