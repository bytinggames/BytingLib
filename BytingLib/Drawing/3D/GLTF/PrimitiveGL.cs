using System.Text.Json.Nodes;

namespace BytingLib
{
    public class PrimitiveGL : PrimitiveGLBase
    {
        private readonly Promise<VertexBuffer> vertexBufferPromise;
        private readonly Promise<IndexBuffer>? indexBufferPromise;

        public VertexBuffer VertexBuffer => vertexBufferPromise.Value;
        public IndexBuffer? IndexBuffer => indexBufferPromise?.Value;

        public PrimitiveGL(ModelGL model, JsonNode n)
            : base(model, n)
        {
            var attributesObj = n["attributes"]!.AsObject();

            string key = string.Concat(attributesObj.Select(f => f.Key + f.Value));

            vertexBufferPromise = model.GetVertexBuffer(key, attributesObj);

            JsonNode? t;
            if ((t = n["indices"]) != null)
            {
                int indicesAccessorIndex = t.GetValue<int>();
                indexBufferPromise = model.GetIndexBuffer(indicesAccessorIndex);
            }
        }

        public void Draw(IShader shader, IShaderMaterial? shaderMaterial)
        {
            using (Material == null ? null : shaderMaterial?.UseMaterial(Material))
            {
                if (IndexBuffer == null)
                {
                    shader.Draw(VertexBuffer);
                }
                else
                {
                    shader.Draw(VertexBuffer, IndexBuffer);
                }
            }
        }
    }
}
