using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public struct Int3 : IComparable<Int3>
    {
        public int X, Y, Z;

        public Int3(int xyz)
        {
            this.X = this.Y = this.Z = xyz;
        }
        public Int3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Int3(Vector3 vec)
        {
            this.X = (int)vec.X;
            this.Y = (int)vec.Y;
            this.Z = (int)vec.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>Returns true, when any value changed.</summary>
        public bool SetInRect(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            int x2 = X;
            int y2 = Y;
            int z3 = Z;
            X = Math.Min(maxX, Math.Max(minX, X));
            Y = Math.Min(maxY, Math.Max(minY, Y));
            Z = Math.Min(maxZ, Math.Max(minZ, Z));

            return x2 != X || y2 != Y || z3 != Z;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj is Int3)
        //    return x == vint.x && Y == vint.y;
        //}

        public static bool operator ==(Int3 v1, Int3 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }
        public static bool operator !=(Int3 v1, Int3 v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
        }

        public static Int3 operator +(Int3 v1, Int3 v2)
        {
            return new Int3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }
        public static Int3 operator -(Int3 v1, Int3 v2)
        {
            return new Int3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public static Int3 operator -(Int3 v1)
        {
            return new Int3(-v1.X, -v1.Y, -v1.Z);
        }
        public static Int3 operator *(Int3 v1, Int3 v2)
        {
            return new Int3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }
        public static Int3 operator *(Int3 v1, float f2)
        {
            return new Int3((int)(v1.X * f2), (int)(v1.Y * f2), (int)(v1.Z * f2));
        }
        public static Int3 operator /(Int3 v1, Int3 v2)
        {
            return new Int3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }

        public static Int3 operator +(Int3 v1, Vector3 v2)
        {
            return new Int3(v1.X + (int)v2.X, v1.Y + (int)v2.Y, v1.Z + (int)v2.Z);
        }

        public static Int3 operator *(Int3 v1, int i2)
        {
            return new Int3(v1.X * i2, v1.Y * i2, v1.Z * i2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return X + " " + Y + " " + Z;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public void SwapXY()
        {
            int save = X;
            X = Y;
            Y = save;
        }

        public void SwapXZ()
        {
            int save = X;
            X = Z;
            Z = save;
        }

        public void SwapYZ()
        {
            int save = Y;
            Y = Z;
            Z = save;
        }

        public int CompareTo([AllowNull] Int3 other)
        {
            int zSign = Math.Sign(Z - other.Z);
            if (zSign == 0)
            {
                int ySign = Math.Sign(Y - other.Y);
                if (ySign == 0)
                {
                    return Math.Sign(X - other.X);
                }
                else
                    return ySign;
            }
            else
                return zSign;
        }
    }
}
