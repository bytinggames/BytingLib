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
    }
}
