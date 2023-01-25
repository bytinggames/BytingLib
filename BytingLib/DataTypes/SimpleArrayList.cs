namespace BytingLib
{
    public class SimpleArrayList<T> : ISimpleArrayList
    {
        private readonly int growBy;
        public T[] Arr;
        public int Count { get; private set; }
        public int Capacity => Arr.Length;

        public SimpleArrayList(int startCapacity = 0, int growBy = 64)
        {
            this.Arr = new T[startCapacity];
            this.growBy = growBy;
        }


        public void EnsureSize(int capacity)
        {
            if (Arr.Length >= capacity)
                return;
            capacity = (int)MathF.Ceiling((float)capacity / growBy) * growBy;
            Array.Resize(ref Arr, capacity);
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Add(T element)
        {
            EnsureSize(Count + 1);
            Arr[Count++] = element;
        }
        public void Add(params T[] elements)
        {
            EnsureSize(Count + elements.Length);
            for (int i = 0; i < elements.Length; i++)
            {
                Arr[Count++] = elements[i];
            }
        }
    }
}
