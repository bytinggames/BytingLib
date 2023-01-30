using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MeshGL
    {
        public readonly string? Name;
        public override string ToString() => "Mesh: " + Name;
        List<Primitive> Primitives = new();

        public MeshGL(ModelGL model, JsonNode n)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["primitives"]) != null)
            {
                JsonArray primitivesArr = t.AsArray();
                for (int i = 0; i < primitivesArr.Count; i++)
                {
                    Primitives.Add(new Primitive(model, primitivesArr[i]!));
                }
            }
        }

        public void Draw(IShaderGLSkinned shader)
        {
            for (int i = 0; i < Primitives.Count; i++)
            {
                Primitives[i].Draw(shader);
            }
        }
    }
}
