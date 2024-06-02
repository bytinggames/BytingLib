using System.Collections;
using System.Text.Json.Nodes;

namespace BytingLib
{
    public class JsonArrayCache<TValue> : IEnumerable<TValue> where TValue : class
    {
        protected TValue?[] dict;
        protected readonly JsonArray container;
        protected Func<JsonNode, TValue> loadFromContainer;

        public JsonArrayCache(JsonArray container, Func<JsonNode, TValue> loadFromContainer)
        {
            this.container = container;
            this.loadFromContainer = loadFromContainer;

            dict = new TValue?[container.Count];
        }

        /// <summary>Including unloaded values.</summary>
        public int TotalCount => container.Count;

        public TValue? Get(int index)
        {
            if (index < 0
                || index >= container.Count)
            {
                return null;
            }

            if (dict[index] != null)
            {
                return dict[index];
            }

            var val = loadFromContainer(container[index]!);
            dict[index] = val;
            return val;
        }

        public void ForEachLoaded(Action<TValue> action)
        {
            for (int i = 0; i < dict.Length; i++)
            {
                if (dict[i] != null)
                {
                    action(dict[i]!);
                }
            }
        }

        public void ForEach(Action<TValue> action)
        {
            for (int i = 0; i < TotalCount; i++)
            {
                action(Get(i)!);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < dict.Length; i++)
            {
                dict[i] = null;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            for (int i = 0; i < TotalCount; i++)
            {
                yield return Get(i)!;
            }
        }

        public IEnumerable<TValue> AsEnumerable()
        {
            for (int i = 0; i < TotalCount; i++)
            {
                yield return Get(i)!;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
