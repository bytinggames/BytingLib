using Microsoft.Xna.Framework;

namespace BytingLib
{
    public static class Vector3Extension
    {
        public static Vector3 GetRound(this Vector3 vec)
        {
            return new Vector3(MathF.Round(vec.X), MathF.Round(vec.Y), MathF.Round(vec.Z));
        }
        public static Vector3 GetFloor(this Vector3 vec)
        {
            return new Vector3(MathF.Floor(vec.X), MathF.Floor(vec.Y), MathF.Floor(vec.Z));
        }
        public static Vector3 GetCeil(this Vector3 vec)
        {
            return new Vector3(MathF.Ceiling(vec.X), MathF.Ceiling(vec.Y), MathF.Ceiling(vec.Z));
        }
        public static Vector3 GetAbs(this Vector3 vec)
        {
            return new Vector3(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
        }
        public static Vector3 GetSign(this Vector3 vec)
        {
            return new Vector3(Math.Sign(vec.X), Math.Sign(vec.Y), Math.Sign(vec.Z));
        }
        public static Vector3 GetMin(this Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(Math.Min(vec1.X, vec2.X), Math.Min(vec1.Y, vec2.Y), Math.Min(vec1.Z, vec2.Z));
        }
        public static Vector3 GetMax(this Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(Math.Max(vec1.X, vec2.X), Math.Max(vec1.Y, vec2.Y), Math.Max(vec1.Z, vec2.Z));
        }
        public static Vector3 GetClamp(this Vector3 vec, Vector3 min, Vector3 max)
        {
            return new Vector3(Math.Max(Math.Min(vec.X, max.X), min.X), Math.Max(Math.Min(vec.Y, max.Y), min.Y), Math.Max(Math.Min(vec.Z, max.Z), min.Z));
        }
        public static Vector3 GetModulate(this Vector3 vec, Vector3 mod)
        {
            return new Vector3(vec.X % mod.X, vec.Y % mod.Y, vec.Z % mod.Z);
        }
        public static Vector3 GetModulate(this Vector3 vec, float mod)
        {
            return new Vector3(vec.X % mod, vec.Y % mod, vec.Z % mod);
        }
        public static Vector3 GetNormalizedOrZero(this Vector3 vec)
        {
            if (vec == Vector3.Zero)
                return Vector3.Zero;
            else
                return Vector3.Normalize(vec);
        }

        public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.X, v.Z);
        public static Vector2 YX(this Vector3 v) => new Vector2(v.Y, v.X);
        public static Vector2 YZ(this Vector3 v) => new Vector2(v.Y, v.Z);
        public static Vector2 ZX(this Vector3 v) => new Vector2(v.Z, v.X);
        public static Vector2 ZY(this Vector3 v) => new Vector2(v.Z, v.Y);
    }
}
