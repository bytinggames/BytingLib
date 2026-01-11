namespace BytingLib
{
    public class StoredRandomFloats
    {
        private readonly Random rand;
        private List<float> floats = new();
        private int index;

        public StoredRandomFloats(Random rand)
        {
            this.rand = rand;
        }

        public void ClearValues()
        {
            index = 0;
            floats.Clear();
        }

        public float Get(Func<Random, float> getNext)
        {
            if (index >= floats.Count)
            {
                float val = getNext(rand);
                floats.Add(val);
                index = floats.Count + 1;
                return val;
            }
            else
            {
                return floats[index++];
            }
        }

        public void Begin()
        {
            index = 0;
        }
    }
}