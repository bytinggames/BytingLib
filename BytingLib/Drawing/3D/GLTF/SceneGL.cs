using System.Text.Json.Nodes;

namespace BytingLib
{
    public class SceneGL : INodeContainer
    {
        public List<NodeGL> Children { get; } = new();

        public SceneGL(ModelGL model, JsonNode n)
        {
            var nodesArr = n["nodes"]!.AsArray();
            for (int i = 0; i < nodesArr.Count; i++)
            {
                int nodeId = nodesArr[i]!.GetValue<int>();
                Children.Add(model.Nodes!.Get(nodeId)!);
            }
        }

        internal void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin)
        {
            for (int i = 0; i < Children.Count; i++)
                Children[i].Draw(shader, shaderMaterial, shaderSkin);
        }

        internal void Draw(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Predicate<NodeGL> goDown)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (goDown(Children[i]))
                    Children[i].Draw(shader, shaderMaterial, shaderSkin, goDown);
            }
        }

        internal void DrawSelect(IShaderWorld shader, IShaderMaterial? shaderMaterial, IShaderSkin? shaderSkin, Predicate<NodeGL> select, Matrix transform)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].DrawSelect(shader, shaderMaterial, shaderSkin, select, transform);
            }
        }
    }
}
