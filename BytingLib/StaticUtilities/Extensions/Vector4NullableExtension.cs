namespace BytingLib
{
    public static class Vector4NullableExtension
    {
        public static Vector4 GetInterpolateTo(this Vector4? from, Vector4 to, float multiplier)
        {
            if (from == null)
            {
                return to;
            }
            return from.Value + (to - from.Value) * multiplier;
        }
    }
}
