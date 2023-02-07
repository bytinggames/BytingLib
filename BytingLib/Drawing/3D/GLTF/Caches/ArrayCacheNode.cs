using System.Text.Json.Nodes;
using BytingLib.DataTypes;

namespace BytingLib
{
    public class ArrayCacheNode : JsonArrayCache<NodeGL>
    {
        private readonly Func<JsonNode, NodeGL?, NodeGL> loadFromContainerAlternative;

        public ArrayCacheNode(JsonArray container, Func<JsonNode, NodeGL?, NodeGL> loadFromContainer)
            :base(container, n => loadFromContainer(n, null))
        {
            loadFromContainerAlternative = loadFromContainer;
        }

        public NodeGL Get(int index, NodeGL? parent)
        {
            NodeGL? value;
            if ((value = dict[index]) != null)
            {
                value.SetParentIfNotHavingOne(parent);
                return value;
            }

            return dict[index] = loadFromContainerAlternative(container[index]!, parent);
        }
    }
}
