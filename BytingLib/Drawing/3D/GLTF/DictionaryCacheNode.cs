using System.Text.Json.Nodes;
using BytingLib.DataTypes;

namespace BytingLib
{
    public class DictionaryCacheNode : DictionaryCache<NodeGL>
    {
        private Func<JsonNode, NodeGL?, NodeGL> loadFromContainerAlternative;

        public DictionaryCacheNode(JsonArray container, Func<JsonNode, NodeGL?, NodeGL> loadFromContainer)
            :base(container, n => loadFromContainer(n, null))
        {
            loadFromContainerAlternative = loadFromContainer;
        }

        public NodeGL Get(int index, NodeGL? parent)
        {
            if (dict.TryGetValue(index, out NodeGL? value))
            {
                value.SetParentIfNotHavingOne(parent);
                return value;
            }

            var val = loadFromContainerAlternative(container[index]!, parent);
            dict.Add(index, val);
            return val;
        }
    }
}
