
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Line3 : IShape3
    {
        public readonly Vector3[] Vertices = new Vector3[2];

        public Line3(Vector3 position, Vector3 direction)
        {
            Vertices[0] = position;
            Vertices[1] = position + direction;
        }

        private Line3()
        {
        }

        public static Line3 FromTwoPoints(Vector3 pos1, Vector3 pos2)
        {
            Line3 line = new Line3();
            line.Vertices[0] = pos1;
            line.Vertices[1] = pos2;

            return line;
        }

        public Vector3 Pos { get => Vertices[0]; set => Vertices[0] = value; }
        public float X { get => Vertices[0].X; set => Vertices[0].X = value; }
        public float Y { get => Vertices[0].Y; set => Vertices[0].Y = value; }
        public float Z { get => Vertices[0].Z; set => Vertices[0].Z = value; }

        public Vector3 Dir
        {
            get => Vertices[1] - Vertices[0];
            set => Vertices[1] = Vertices[0] + value;
        }

        public Vector3 DirN => Vector3.Normalize(Dir);

        public Vector3 Pos2
        {
            get => Vertices[1];
            set => Vertices[1] = value;
        }

        public object Clone()
        {
            Line3 clone = (Line3)MemberwiseClone();

            clone.Vertices[0] = Vertices[0];
            clone.Vertices[1] = Vertices[1];

            return clone;
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(Vector3.Min(Pos, Pos2), Vector3.Max(Pos, Pos2));
        }
    }
}
