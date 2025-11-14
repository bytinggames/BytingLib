namespace BytingLib
{
    public static class Vector3NullableExtension
    {
        public static Vector3 GetInterpolateTo(this Vector3? from, Vector3 to, float multiplier)
        {
            if (from == null)
            {
                return to;
            }
            return from.Value + (to - from.Value) * multiplier;
        }
    }
}
