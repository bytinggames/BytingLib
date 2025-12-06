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
        /// <summary>
        /// Remap a float value in an input range proportional to a corresponding output range
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="rangeInMin">Input range lower bound</param>
        /// <param name="rangeInMax">Input range upper bound</param>
        /// <param name="rangeOutMin">Output range lower bound</param>
        /// <param name="rangeOutMax">Output range upper bound</param>
        public static float MapRange(this float value, float rangeInMin, float rangeInMax, float rangeOutMin, float rangeOutMax)
        {
            float mix = (value - rangeInMin) / (rangeInMax - rangeInMin);
            return mix * (rangeOutMax - rangeOutMin) + rangeOutMin;
        }

        /// <summary>
        /// Remap a float value in an input range to a corresponding output range. Clamp output to the bounds of the output range.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="rangeInMin">Input range lower bound</param>
        /// <param name="rangeInMax">Input range upper bound</param>
        /// <param name="rangeOutMin">Output range lower bound</param>
        /// <param name="rangeOutMax">Output range upper bound</param>
        public static float MapRangeClamped(this float value, float rangeInMin, float rangeInMax, float rangeOutMin, float rangeOutMax)
        {
            return Math.Clamp(value.MapRange(rangeInMin, rangeInMax, rangeOutMin, rangeOutMax), Math.Min(rangeOutMin, rangeOutMax), Math.Max(rangeOutMin, rangeOutMax));
        }

        public static bool NearlyEqual(this float a, float b, float maxDifference)
        {
            return MathF.Abs(a - b) <= maxDifference;
        }

        public static float Clamp(this float f, float min, float max)
        {
            return Math.Clamp(f, min, max);
        }
        public static float Clamp(this float f, float maxAndMinNegative)
        {
            return Math.Clamp(f, -maxAndMinNegative, maxAndMinNegative);
        }

        public static float Wrap(this float n, float wrapRange)
        {
            n %= wrapRange;
            if (n < 0)
            {
                n += wrapRange;
            }
            return n;
        }
    }
}
