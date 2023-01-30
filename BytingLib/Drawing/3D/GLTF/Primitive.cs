using System.Text.Json.Nodes;

namespace BytingLib
{
    public class Primitive
    {
        public VertexBuffer VertexBuffer;
        public IndexBuffer? IndexBuffer;
        public MaterialGL? Material;

        public Primitive(ModelGL model, JsonNode n)
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

        public void Draw(IShaderDefault shader)
        {
            string techniqueName = Shader.GetTechniqueName(VertexBuffer.VertexDeclaration);

            using (shader.UseTechnique(techniqueName))
            {
                using (Material?.Use(shader))
                {
                    if (IndexBuffer == null)
                        shader.Draw(VertexBuffer);
                    else
                        shader.Draw(VertexBuffer, IndexBuffer);
                }
            }
        }
    }
}
