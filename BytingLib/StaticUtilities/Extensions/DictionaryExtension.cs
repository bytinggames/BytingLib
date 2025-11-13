namespace BytingLib
{
    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            Func<TKey, TValue, bool> predicate)
        {
            var keys = dict.Keys.Where(k => predicate(k, dict[k])).ToList();
            foreach (var key in keys)
            {
                dict.Remove(key);
            }
        }
        public static void RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            Func<TKey, bool> predicate)
        {
            var keys = dict.Keys.Where(k => predicate(k)).ToList();
            foreach (var key in keys)
            {
                dict.Remove(key);
            }
        }
    }
}
