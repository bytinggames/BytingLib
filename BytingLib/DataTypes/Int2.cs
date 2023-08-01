using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public struct Int2 : IComparable<Int2>
    {
        public int X, Y;

        public static Int2 Zero { get; } = new Int2(0, 0);
        public static Int2 One { get; } = new Int2(1);
        public static Int2 UnitX { get; } = new Int2(1, 0);
        public static Int2 UnitY { get; } = new Int2(0, 1);

        public Int2(int xy)
        {
            this.X = this.Y = xy;
        }
        public Int2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Int2(Vector2 vec)
        {
            this.X = (int)vec.X;
            this.Y = (int)vec.Y;
        }

        public Int2(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public bool SetInRect(int minX, int minY, int maxX, int maxY)
        {
            int x2 = X;
            int y2 = Y;
            X = Math.Min(maxX, Math.Max(minX, X));
            Y = Math.Min(maxY, Math.Max(minY, Y));

            return x2 != X || y2 != Y;
        }

        public static bool operator ==(Int2 v1, Int2 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }
        public static bool operator !=(Int2 v1, Int2 v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }

        public static Int2 operator +(Int2 v1, Int2 v2)
        {
            return new Int2(v1.X + v2.X, v1.Y + v2.Y);
        }
        public static Int2 operator -(Int2 v1, Int2 v2)
        {
            return new Int2(v1.X - v2.X, v1.Y - v2.Y);
        }
        public static Int2 operator -(Int2 v1)
        {
            return new Int2(-v1.X, -v1.Y);
        }
        public static Int2 operator *(Int2 v1, Int2 v2)
        {
            return new Int2(v1.X * v2.X, v1.Y * v2.Y);
        }
        public static Int2 operator *(Int2 v1, float f2)
        {
            return new Int2((int)(v1.X * f2), (int)(v1.Y * f2));
        }
        public static Int2 operator /(Int2 v1, Int2 v2)
        {
            return new Int2(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Int2 operator +(Int2 v1, Vector2 v2)
        {
            return new Int2(v1.X + (int)v2.X, v1.Y + (int)v2.Y);
        }

        public static Int2 operator *(Int2 v1, int i2)
        {
            return new Int2(v1.X * i2, v1.Y * i2);
        }
        public static Int2 operator /(Int2 v1, int i2)
        {
            return new Int2(v1.X / i2, v1.Y / i2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Int2 f)
                return f.X == X && f.Y == Y;
            return false;
        }

        public override string ToString()
        {
            return X + " " + Y;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public void SwapXY()
        {
            int save = X;
            X = Y;
            Y = save;
        }

        public int CompareTo([AllowNull] Int2 other)
        {
            int ySign = Math.Sign(Y - other.Y);
            if (ySign == 0)
            {
                return Math.Sign(X - other.X);
            }
            else
                return ySign;
        }
    }
}
