using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MeshGL
    {
        public string? Name { get; set; }
        public List<PrimitiveGL> Primitives { get; } = new();

        public MeshGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["primitives"]) != null)
            {
                JsonArray primitivesArr = t.AsArray();
                for (int i = 0; i < primitivesArr.Count; i++)
                {
                    Primitives.Add(new PrimitiveGL(model, primitivesArr[i]!));
                }
            }
        }

        public override string ToString() => "Mesh: " + Name;

        public void Draw(IShaderGL shader)
        {
            for (int i = 0; i < Primitives.Count; i++)
            {
                Primitives[i].Draw(shader);
            }
        }
    }
}
