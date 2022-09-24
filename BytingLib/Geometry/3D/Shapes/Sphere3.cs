
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Sphere3 : IShape3
    {
        private Vector3 pos;
        public float Radius;

        public Sphere3(Vector3 position, float radius)
        {
            pos = position;
            Radius = radius;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public Type GetCollisionType() => typeof(Sphere3);

        public virtual object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            Vector3 r = new Vector3(Radius);
            return new BoundingBox(Pos - r, Pos + r);
        }
    }
}
