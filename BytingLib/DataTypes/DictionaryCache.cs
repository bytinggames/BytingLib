using System.Text.Json.Nodes;

namespace BytingLib.DataTypes
{
    public class DictionaryCache<TValue>
    {
        protected Dictionary<int, TValue> dict = new();
        protected readonly JsonArray container;
        protected Func<JsonNode, TValue> loadFromContainer;

        public DictionaryCache(JsonArray container, Func<JsonNode, TValue> loadFromContainer)
        {
            this.container = container;
            this.loadFromContainer = loadFromContainer;
        }

        public TValue Get(int index)
        {
            if (dict.TryGetValue(index, out TValue? value))
                return value;

            var val = loadFromContainer(container[index]!);
            dict.Add(index, val);
            return val;
        }

        public void ForEach(Action<TValue> action)
        {
            foreach (var val in dict.Values)
            {
                action(val);
            }
        }

        public void Clear()
        {
            dict.Clear();
        }
    }
}
