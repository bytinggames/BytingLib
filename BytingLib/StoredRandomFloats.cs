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

        public float NextSingle(float min, float max)
        {
            if (index >= floats.Count)
            {
                return Add(rand.NextSingle(min, max));
            }
            else
            {
                return floats[index++];
            }
        }

        public float NextSinglePow(float min, float max, float power)
        {
            if (index >= floats.Count)
            {
                return Add(rand.NextSinglePow(min, max, power));
            }
            else
            {
                return floats[index++];
            }
        }

        private float Add(float val)
        {
            floats.Add(val);
            index = floats.Count + 1;
            return val;
        }

        public void Begin()
        {
            index = 0;
        }
    }
}