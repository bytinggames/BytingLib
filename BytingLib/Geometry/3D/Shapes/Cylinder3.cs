
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Cylinder3 : IShape3
    {
        private Vector3 pos;
        public Vector3 Length;
        public float Radius;

        public Cylinder3(Vector3 pos, Vector3 length, float radius)
        {
            this.pos = pos;
            Length = length;
            Radius = radius;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }
        public Vector3 Pos2 => Pos + Length;

        public Type GetCollisionType() => typeof(Cylinder3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 a = Length;
            Vector3 e = Radius * Vector3Extension.GetSqrt(Vector3.One - a * a / Vector3.Dot(a, a));
            return new BoundingBox(Vector3.Min(Pos - e, Pos + Length - e)
                , Vector3.Max(Pos + e, Pos + Length + e));
        }
    }
}
