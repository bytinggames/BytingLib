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
            Vector2 v = Vector2.Normalize(vec);
            if (float.IsFinite(v.X))
            {
                return v;
            }
            return Vector2.Zero;
        }
        public static Vector2 GetNormalizedOr(this Vector2 vec, Vector2 or)
        {
            Vector2 v = Vector2.Normalize(vec);
            if (float.IsFinite(v.X))
            {
                return v;
            }
            return or;
        }
        internal static Vector2 XPositive(this Vector2 vec)
        {
            if (vec.X < 0 || (vec.X == 0 && vec.Y < 0))
            {
                return -vec;
            }

            return vec;
        }
        internal static Vector2 YPositive(this Vector2 vec)
        {
            if (vec.Y < 0 || (vec.Y == 0 && vec.X < 0))
            {
                return -vec;
            }

            return vec;
        }
        public static Vector2 GetRotate90(this Vector2 vec)
        {
            return new Vector2(-vec.Y, vec.X);
        }
        public static Vector2 GetRotate90CounterClock(this Vector2 vec)
        {
            return new Vector2(vec.Y, -vec.X);
        }
        public static Vector2 GetMoveTo(this Vector2 val, Vector2 goal, float speed)
        {
            if (val == goal)
            {
                return val;
            }

            Vector2 dist = goal - val;
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
        public static float ToAngle(this Vector2 vec)
        {
            return MathF.Atan2(vec.Y, vec.X);
        }
        public static Vector2 AngleToVector(this float angle)
        {
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
        public static float AngleTo(this Vector2 vec1, Vector2 vec2)
        {
            return MathExtension.AngleDistance(vec1.ToAngle(), vec2.ToAngle());
        }
        public static float AngleToAbs(this Vector2 vec1, Vector2 vec2)
        {
            return MathF.Abs(AngleTo(vec1, vec2));
        }
        public static float Average(this Vector2 v) => (v.X + v.Y) / 2f;
        public static float AspectRatio(this Vector2 v) => v.X / v.Y;

        public static Vector2 GetSqrt(this Vector2 v)
        {
            return new Vector2(MathF.Sqrt(v.X), MathF.Sqrt(v.Y));
        }

        public static Vector2 FromArray(float[] arr)
        {
            return new Vector2(arr[0], arr[1]);
        }

        public static Vector4 XYXY(this Vector2 v)
        {
            return new Vector4(v.X, v.Y, v.X, v.Y);
        }


        static readonly Vector2[] dir8 = [
            new Vector2(1f, 0f),
            Vector2.Normalize(new Vector2(1f, 1f)),
            new Vector2(0f, 1f),
            Vector2.Normalize(new Vector2(-1f, 1f)),
            new Vector2(-1f, 0f),
            Vector2.Normalize(new Vector2(-1f, -1f)),
            new Vector2(0f, -1f),
            Vector2.Normalize(new Vector2(1f, -1f)),
            ];
        public static Vector2 To8Dir(this Vector2 v)
        {
            float angle = v.ToAngle();
            // make positive
            angle = (angle + MathHelper.TwoPi) % MathHelper.TwoPi;
            int angleIndex = (int)MathF.Round(angle / MathHelper.PiOver4);
            angleIndex = Math.Clamp(angleIndex, 0, 7);
            return dir8[angleIndex];
        }

        internal static Vector2? GetMax(IEnumerable<Vector2> enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return null;
            }
            Vector2 v = enumerator.Current;
            while (enumerator.MoveNext())
            {
                v = v.GetMax(enumerator.Current);
            }
            return v;
        }
    }
}
