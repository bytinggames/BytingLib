using Microsoft.Xna.Framework;

namespace BytingLib
{
    public static class Vector2Extension
    {
        public static Vector2 GetRound(this Vector2 vec)
        {
            return new Vector2(MathF.Round(vec.X), MathF.Round(vec.Y));
        }
        public static Vector2 GetFloor(this Vector2 vec)
        {
            return new Vector2(MathF.Floor(vec.X), MathF.Floor(vec.Y));
        }
        public static Vector2 GetCeil(this Vector2 vec)
        {
            return new Vector2(MathF.Ceiling(vec.X), MathF.Ceiling(vec.Y));
        }
        public static Vector2 GetAbs(this Vector2 vec)
        {
            return new Vector2(Math.Abs(vec.X), Math.Abs(vec.Y));
        }
        public static Vector2 GetSign(this Vector2 vec)
        {
            return new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y));
        }
        public static Vector2 GetMin(this Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(Math.Min(vec1.X, vec2.X), Math.Min(vec1.Y, vec2.Y));
        }
        public static Vector2 GetMax(this Vector2 vec1, Vector2 vec2)
        {
            return new Vector2(Math.Max(vec1.X, vec2.X), Math.Max(vec1.Y, vec2.Y));
        }
        public static Vector2 GetClamp(this Vector2 vec, Vector2 min, Vector2 max)
        {
            return new Vector2(Math.Max(Math.Min(vec.X, max.X), min.X), Math.Max(Math.Min(vec.Y, max.Y), min.Y));
        }
        public static Vector2 GetModulate(this Vector2 vec, Vector2 mod)
        {
            return new Vector2(vec.X % mod.X, vec.Y % mod.Y);
        }
        public static Vector2 GetModulate(this Vector2 vec, float mod)
        {
            return new Vector2(vec.X % mod, vec.Y % mod);
        }
        public static Vector2 GetNormalizedOrZero(this Vector2 vec)
        {
            if (vec == Vector2.Zero)
                return Vector2.Zero;
            else
                return Vector2.Normalize(vec);
        }
        internal static Vector2 XPositive(this Vector2 vec)
        {
            if (vec.X < 0 || (vec.X == 0 && vec.Y < 0))
                return -vec;
            return vec;
        }
        internal static Vector2 YPositive(this Vector2 vec)
        {
            if (vec.Y < 0 || (vec.Y == 0 && vec.X < 0))
                return -vec;
            return vec;
        }
    }
}
