namespace BytingLib
{
    public static class FloatNullableExtension
    {
        public static float GetInterpolateTo(this float? from, float to, float multiplier)
        {
            if (from == null)
            {
                return to;
            }
            return from.Value + (to - from.Value) * multiplier;
        }
    }
}
