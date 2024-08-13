using System.Text.Json.Nodes;

namespace BytingLib
{
    public class MeshGL
    {
        public string? Name { get; set; }
        /// <summary>Only used when a GraphicsDevice is available</summary>
        public List<PrimitiveGL>? Primitives { get; }
        /// <summary>Only used with the Pipeline, when no GraphicsDevice is available</summary>
        public List<PrimitiveGLContent>? PrimitivesContent { get; }

        public MeshGL(ModelGL model, JsonNode n, bool graphicsDeviceAvailable)
        {
            Name = n["name"]?.GetValue<string>();

            JsonNode? t;
            if ((t = n["primitives"]) != null)
            {
                JsonArray primitivesArr = t.AsArray();

                if (graphicsDeviceAvailable)
                {
                    Primitives = new();
                }
                else
                {
                    PrimitivesContent = new();
                }

                for (int i = 0; i < primitivesArr.Count; i++)
                {
                    if (graphicsDeviceAvailable)
                    {
                        Primitives!.Add(new PrimitiveGL(model, primitivesArr[i]!));
                    }
                    else
                    {

                        try
                        {
                            PrimitivesContent!.Add(new PrimitiveGLContent(model, primitivesArr[i]!));
                        }
                        catch (VertexAttributeNotSupportedExeption e)
                        {
                            throw new VertexAttributeNotSupportedExeption(e.Message + " in mesh " + Name + ". This mesh might only exist in the .export.blend file that was generated when baking.", e);
                        }
                    }
                }
            }
        }

        public override string ToString() => "Mesh: " + Name;

        public void Draw(IShader shader, IShaderMaterial? shaderMaterial)
        {
            if (Primitives != null)
            {
                for (int i = 0; i < Primitives.Count; i++)
                {
                    Primitives[i].Draw(shader, shaderMaterial);
                }
            }
        }
    }
}
