using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public class IndexedDictionary<Key, Value> : IDictionary<Key, Value> where Key : notnull
    {
        private readonly IDictionary<Key, int> dictionary;
        private readonly List<(Key Key, Value Value)> list = new();

        public IndexedDictionary()
            : this(EqualityComparer<Key>.Default)
        {
        }

        public IndexedDictionary(IEqualityComparer<Key> comparer)
        {
            dictionary = new Dictionary<Key, int>(comparer);
        }

        public int Count => dictionary.Count;

        public virtual bool IsReadOnly => dictionary.IsReadOnly;

        public ICollection<Key> Keys => throw new NotImplementedException();

        public ICollection<Value> Values => throw new NotImplementedException();

        public bool Add(Key key, Value val)
        {
            if (!dictionary.TryAdd(key, list.Count))
            {
                return false;
            }
            list.Add((key, val));
            return true;
        }

        public bool Add(Key key, Value val, out int index)
        {
            if (dictionary.TryGetValue(key, out index))
            {
                return false;
            }

            dictionary.Add(key, list.Count);
            list.Add((key, val));
            return true;
        }

        public void Clear()
        {
            list.Clear();
            dictionary.Clear();
        }

        public bool Remove(Key key)
        {
            if (key == null)
            {
                return false;
            }
            if (!dictionary.TryGetValue(key, out int index))
            {
                return false;
            }
            dictionary.Remove(key);
            list.RemoveAt(index);
            return true;
        }

        public IEnumerator<(Key, Value)> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(Key item)
        {
            return item != null && dictionary.ContainsKey(item);
        }

        public void CopyTo(Value[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public (Key Key, Value Value) this[int index]
        {
            get
            {
                return list[index];
            }
        }

        Value IDictionary<Key, Value>.this[Key key]
        {
            get
            {
                int index = dictionary[key];
                return list[index].Value;
            }
            set
            {
                list[dictionary[key]] = new(key, value);
            }
        }

        //public (int Index, Value Value) this[Key key]
        //{
        //    get
        //    {
        //        int index = dictionary[key];
        //        return (index, list[index].Value);
        //    }
        //}

        public int IndexOf(Key item)
        {
            if (item == null
                || !dictionary.TryGetValue(item, out int index))
            {
                return -1;
            }
            return index;
        }

        void IDictionary<Key, Value>.Add(Key key, Value val)
        {
            if (dictionary.TryAdd(key, list.Count))
            {
                list.Add((key, val));
            }
        }

        public bool ContainsKey(Key key) => dictionary.ContainsKey(key);


        public bool TryGetValue(Key key, [MaybeNullWhen(false)] out Value value) => TryGetValue(key, out value, out _);
        public bool TryGetValue(Key key, [MaybeNullWhen(false)] out Value value, out int index)
        {
            if (dictionary.TryGetValue(key, out index))
            {
                value = list[index].Value;
                return true;
            }
            index = -1;
            value = default;
            return false;
        }

        public void Add(KeyValuePair<Key, Value> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<Key, Value> item)
        {
            if (dictionary.TryGetValue(item.Key, out int index))
            {
                return list[index]!.Equals(item.Value);
            }
            return false;
        }

        public void CopyTo(KeyValuePair<Key, Value>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<Key, Value> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<Key, Value>> IEnumerable<KeyValuePair<Key, Value>>.GetEnumerator()
        {
            foreach (var keyValue in dictionary)
            {
                yield return new(keyValue.Key, list[keyValue.Value].Value);
            }
        }
    }
}
