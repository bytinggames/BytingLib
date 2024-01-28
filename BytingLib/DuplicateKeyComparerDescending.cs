namespace BytingLib
{
    /// <summary>The inverse of <see cref="DuplicateKeyComparer{TKey}"/>.</summary>
    public class DuplicateKeyComparerDescending<TKey>
                    : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey? x, TKey? y)
        {
            if (x == null || y == null) // not sure if this is okay
            {
                return -1;
            }

            int result = x.CompareTo(y);

            if (result == 0)
            {
                return -1; // Handle equality as being greater. Note: this will break Remove(key) or
            }
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            {
                return -result;
            }
        }
    }
}
