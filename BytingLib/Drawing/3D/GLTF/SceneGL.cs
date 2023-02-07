using System.Text.Json.Nodes;

namespace BytingLib
{
    public class SceneGL
    {
        List<NodeGL> nodes = new();

        public SceneGL(ModelGL model, JsonNode n)
        {
            var nodesArr = n["nodes"]!.AsArray();
            for (int i = 0; i < nodesArr.Count; i++)
            {
                int nodeId = nodesArr[i]!.GetValue<int>();
                nodes.Add(model.Nodes!.Get(nodeId));
            }
        }

        internal void Draw(IShaderGL shader)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Draw(shader);
        }

        internal NodeGL? FindNode(string name)
        {
            NodeGL? node;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == name)
                    return nodes[i];
                node = nodes[i].FindNode(name);
                if (node != null)
                    return node;
            }
            return null;
        }
    }
}
