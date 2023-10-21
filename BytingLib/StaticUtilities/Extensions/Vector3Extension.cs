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
            {
                return Vector3.Zero;
            }
            else
            {
                return Vector3.Normalize(vec);
            }
        }
        public static Vector3 GetMoveTo(this Vector3 val, Vector3 goal, float speed)
        {
            if (val == goal)
            {
                return val;
            }

            Vector3 dist = goal - val;
            float distLength = dist.Length();
            if (distLength < speed)
            {
                return goal;
            }
            else
            {
                return val + dist * speed / distLength;
            }
        }
        public static float AngleTo(this Vector3 vec1, Vector3 vec2)
        {
            float dot = Vector3.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
            return MathF.Acos(dot);
        }
        public static Vector3 GetNonParallelVector(this Vector3 v)
        {
            if (v.X == 0 && v.Y == 0)
            {
                return new Vector3(0, v.Z, 0);
            }
            else
            {
                return new Vector3(-v.Y, v.X, v.Z);
            }
            // see https://math.stackexchange.com/a/3122025
        }
        public static Vector3 GetSqrt(this Vector3 v)
        {
            return new Vector3(MathF.Sqrt(v.X), MathF.Sqrt(v.Y), MathF.Sqrt(v.Z));
        }

        public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);
        public static Vector2 XZ(this Vector3 v) => new Vector2(v.X, v.Z);
        public static Vector2 YX(this Vector3 v) => new Vector2(v.Y, v.X);
        public static Vector2 YZ(this Vector3 v) => new Vector2(v.Y, v.Z);
        public static Vector2 ZX(this Vector3 v) => new Vector2(v.Z, v.X);
        public static Vector2 ZY(this Vector3 v) => new Vector2(v.Z, v.Y);

        public static void SetXY(ref this Vector3 v, Vector2 xy) { v.X = xy.X; v.Y = xy.Y; }
        public static void SetXZ(ref this Vector3 v, Vector2 xz) { v.X = xz.X; v.Z = xz.Y; }

        public static Vector3 Slerp(this Vector3 start, Vector3 end, float amount)
        {
            if (start == end)
            {
                return end;
            }

            // source: https://stackoverflow.com/a/67920029/6866837
            // Dot product - the cosine of the angle between 2 vectors.
            float dot = Vector3.Dot(start, end);

            // Clamp it to be in the range of Acos()
            // This may be unnecessary, but floating point
            // precision can be a fickle mistress.
            dot = MathF.Min(1f, MathF.Max(-1f, dot));

            // Acos(dot) returns the angle between start and end,
            // And multiplying that by percent returns the angle between
            // start and the final result.
            float theta = MathF.Acos(dot) * amount;
            Vector3 RelativeVec = end - start * dot;
            RelativeVec.Normalize();

            // Orthonormal basis
            // The final result.
            return ((start * MathF.Cos(theta)) + (RelativeVec * MathF.Sin(theta)));
        }

        public static float Average(this Vector3 v) => (v.X + v.Y + v.Z) / 3f;

        public static Vector3 FromArray(float[] arr)
        {
            return new Vector3(arr[0], arr[1], arr[2]);
        }

        public static Vector3 BlenderToGame(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, -v.Y);
        }

        public static Vector3 GameToBlender(this Vector3 v)
        {
            return new Vector3(v.X, -v.Z, v.Y);
        }
    }
}
