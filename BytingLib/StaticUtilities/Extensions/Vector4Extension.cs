namespace BytingLib
{
    public static class Vector4Extension
    {
        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static Vector2 XY(this Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector4 Absolute(Vector2 v)
        {
            return new Vector4(v, 0f, 0f);
        }
        public static Vector4 Absolute(float x, float y)
        {
            return new Vector4(x, y, 0f, 0f);
        }
        public static Vector4 Relative(Vector2 delta)
        {
            return new Vector4(0f, 0f, delta.X, delta.Y);
        }
        public static Vector4 Relative(float x, float y)
        {
            return new Vector4(0f, 0f, x, y);
        }

        public static void MakeAbsolute(this Vector4 v, GameSpeed? gameSpeed)
        {
            float factor = gameSpeed?.Factor ?? 1f;
            v.X += v.Z * factor;
            v.Y += v.W * factor;
            v.Z = 0f;
            v.W = 0f;
        }
        public static Vector2 GetAbsolute(this Vector4 v, GameSpeed? gameSpeed)
        {
            float factor = gameSpeed?.Factor ?? 1f;
            return new Vector2(v.X + v.Z * factor, v.Y + v.W * factor);
        }
        public static Vector2 GetAbsolute(this Vector4 v, float factor)
        {
            return new Vector2(v.X + v.Z * factor, v.Y + v.W * factor);
        }
    }
}
