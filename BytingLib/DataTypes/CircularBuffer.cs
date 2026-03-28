using System.Collections;

namespace SE
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private int head = 0;
        private int count = 0;

        public int Capacity => buffer.Length;
        public int Count => count;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            buffer = new T[capacity];
        }

        public void Add(T item)
        {
            buffer[head] = item;
            head = (head + 1) % buffer.Length;

            if (count < buffer.Length)
            {
                count++;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                int actualIndex = GetInternalIndex(index);
                return buffer[actualIndex];
            }
            set
            {
                if (index < 0 || index >= count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                int actualIndex = GetInternalIndex(index);
                buffer[actualIndex] = value;
            }
        }

        public T? LastOrDefault()
        {
            if (count == 0)
            {
                return default;
            }
            int index = (head - 1 + buffer.Length) % buffer.Length;
            return buffer[index];
        }

        public void ClearAndRelease()
        {
            for (int i = 0; i < count; i++)
            {
                buffer[GetInternalIndex(i)] = default!;
            }
            head = count = 0;
        }
        public void Clear()
        {
            head = count = 0;
        }

        public void RemoveRange(int toRemove)
        {
            if (toRemove > count)
            {
                toRemove = count;
            }
            count -= toRemove;
            head = (head - toRemove + buffer.Length) % buffer.Length;
        }

        public void RemoveRangeAndRelease(int toRemove)
        {
            if (toRemove > count)
            {
                toRemove = count;
            }
            count -= toRemove;

            while (toRemove > 0)
            {
                head--;
                if (head < 0)
                {
                    head = buffer.Length - 1;
                }
                buffer[head] = default!;
                toRemove--;
            }
        }

        private int GetInternalIndex(int index)
        {
            return (head - count + index + buffer.Length) % buffer.Length;
        }

        public IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < count; i++)
            {
                yield return buffer[GetInternalIndex(i)];
            }
        }
        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
