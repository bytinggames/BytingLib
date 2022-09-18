
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Point3 : IShape3
    {
        private Vector3 pos;

        public Point3(Vector3 pos)
        {
            this.pos = pos;
        }

        public Vector3 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Z { get => pos.Z; set => pos.Z = value; }

        public object Clone() => MemberwiseClone();

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Pos, Pos);
        }
    }
}
