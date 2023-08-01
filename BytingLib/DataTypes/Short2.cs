namespace BytingLib
{
    public struct Short2
    {
        private short x, y;

        public short X
        {
            get { return x; }
            set { x = value; }
        }

        public short Y
        {
            get { return y; }
            set { y = value; }
        }

        public Short2(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public Short2(Vector2 vec)
        {
            this.x = (short)vec.X;
            this.y = (short)vec.Y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public bool SetInRect(short minX, short minY, short maxX, short maxY)
        {
            short x2 = x;
            short y2 = y;
            x = Math.Min(maxX, Math.Max(minX, x));
            y = Math.Min(maxY, Math.Max(minY, y));

            return x2 != x || y2 != y;
        }

        public static bool operator ==(Short2 v1, Short2 v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }
        public static bool operator !=(Short2 v1, Short2 v2)
        {
            return v1.x != v2.x || v1.y != v2.y;
        }

        public static Short2 operator +(Short2 v1, Short2 v2)
        {
            return new Short2((short)(v1.X + v2.X), (short)(v1.Y + v2.Y));
        }
        public static Short2 operator -(Short2 v1, Short2 v2)
        {
            return new Short2((short)(v1.X - v2.X), (short)(v1.Y - v2.Y));
        }
        public static Short2 operator *(Short2 v1, Short2 v2)
        {
            return new Short2((short)(v1.X * v2.X), (short)(v1.Y * v2.Y));
        }
        public static Short2 operator /(Short2 v1, Short2 v2)
        {
            return new Short2((short)(v1.X / v2.X), (short)(v1.Y / v2.Y));
        }

        public static Short2 operator +(Short2 v1, Vector2 v2)
        {
            return new Short2((short)(v1.X + (short)v2.X), (short)(v1.Y + (short)v2.Y));
        }

        public static Short2 operator *(Short2 v1, int i2)
        {
            return new Short2((short)(v1.x * i2), (short)(v1.y * i2));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Short2 f)
                return f.X == X && f.Y == Y;
            return false;
        }

        public override string ToString()
        {
            return x + " " + y;
        }

        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }
    }
}
