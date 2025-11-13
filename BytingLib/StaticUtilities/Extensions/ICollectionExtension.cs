
namespace BytingLib
{
    public static class ICollectionExtension
    {
        public static void RemoveRange<T>(this ICollection<T> list, IEnumerable<T> toRemove)
        {
            foreach (var item in toRemove)
            {
                list.Remove(item);
            }
        }
    }
}
