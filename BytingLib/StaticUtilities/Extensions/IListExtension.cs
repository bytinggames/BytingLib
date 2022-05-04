
namespace BytingLib
{
    public static class IListExtension
    {
        public static void AddShuffled<T>(this IList<T> listReceive, IList<T> listGive, Random rand)
        {
            for (int i = 0; i < listGive.Count; i++)
            {
                listReceive.Insert(rand.Next(listReceive.Count + 1), listGive[i]);
            }
        }

        public static void Shuffle<T>(this IList<T> list, Random rand)
        {
            int c = list.Count;
            // duplicate elements randomly to the end of the list (first half is old, last half is randomly new)
            for (int i = 0; i < c; i++)
            {
                list.Insert(c + rand.Next(i + 1), list[i]);
            }
            // remove old half
            while (c > 0)
            {
                list.RemoveAt(0);
                c--;
            }
        }

        public static T RemoveAtGet<T>(this IList<T> list, int index)
        {
            T get = list[index];
            list.RemoveAt(index);
            return get;
        }
    }
}
