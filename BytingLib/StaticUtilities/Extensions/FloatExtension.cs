namespace BytingLib
{
    public static class FloatExtension
    {
        public static unsafe float GetIncrement(this float f)
        {
            int val = *(int*)&f;
            if (f > 0)
            {
                val++;
            }
            else if (f < 0)
            {
                val--;
            }
            else if (f == 0)
            {
                return float.Epsilon;
            }

            return *(float*)&val;
        }
        public static unsafe float Decrement(this float f)
        {
            int val = *(int*)&f;
            if (f > 0)
            {
                val--;
            }
            else if (f < 0)
            {
                val++;
            }
            else if (f == 0)
            {
                return -float.Epsilon; // thanks to Sebastian Negraszus
            }

            return *(float*)&val;
        }
    }
}
