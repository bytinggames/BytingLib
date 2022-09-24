
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class AABB3 : IShape3
    {
        private Vector3 pos;
        public Vector3 Size;

        public AABB3(Vector3 pos, Vector3 size)
        {
            Pos = pos;
            Size = size;
        }

        public static AABB3 FromCenter(Vector3 centerPos, Vector3 size)
        {
            return new AABB3(centerPos - size / 2f, size);
        }

        public virtual Vector3 Pos { get => pos; set => pos = value; }
        public virtual float X { get => pos.X; set => pos.X = value; }
        public virtual float Y { get => pos.Y; set => pos.Y = value; }
        public virtual float Z { get => pos.Z; set => pos.Z = value; }
        public virtual Vector3 Min => pos;
        public virtual Vector3 Max => pos + Size;
        public Vector3 Center
        {
            get => Pos + Size / 2f;
            set => Pos = value - Size / 2f;
        }

        public Type GetCollisionType() => typeof(AABB3);

        public virtual object Clone()
        {
            return new AABB3(pos, Size);
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Min, Max);
        }

        public Vector3 MoveVectorInside(Vector3 pos)
        {
            Vector3 nearestInBox;
            if (pos.X < Min.X)
                nearestInBox.X = Min.X;
            else if (pos.X > Max.X)
                nearestInBox.X = Max.X;
            else
                nearestInBox.X = pos.X;

            if (pos.Y < Min.Y)
                nearestInBox.Y = Min.Y;
            else if (pos.Y > Max.Y)
                nearestInBox.Y = Max.Y;
            else
                nearestInBox.Y = pos.Y;

            if (pos.Z < Min.Z)
                nearestInBox.Z = Min.Z;
            else if (pos.Z > Max.Z)
                nearestInBox.Z = Max.Z;
            else
                nearestInBox.Z = pos.Z;
            return nearestInBox;
        }
    }
}
