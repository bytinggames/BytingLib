namespace BytingLib
{
    public static class Vector2NullableExtension
    {
        public static Vector2 GetInterpolateTo(this Vector2? from, Vector2 to, float multiplier)
        {
            if (from == null)
            {
                return to;
            }
            return from.Value + (to - from.Value) * multiplier;
        }
    }
}
