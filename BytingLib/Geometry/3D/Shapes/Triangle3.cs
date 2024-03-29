﻿namespace BytingLib
{
    public class Triangle3 : IShape3
    {
        public readonly Vector3[] Vertices;

        public Triangle3(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            Vertices = new Vector3[]
            {
                vertex1,
                vertex2,
                vertex3
            };
        }

        public static Triangle3 Create(Vector3 pos, Vector3 a, Vector3 b) => new Triangle3(pos, pos + a, pos + b);

        public Triangle3(Triangle3 tri)
        {
            Vertices = new Vector3[3];
            tri.Vertices.CopyTo(Vertices, 0);
        }

        public Vector3 Pos
        {
            get => this[0];
            set
            {
                Vector3 relative = value - Vertices[0];
                Vertices[0] = value;
                Vertices[1] += relative;
                Vertices[2] += relative;
            }
        }
        public float X
        {
            get => Vertices[0].X;
            set
            {
                float relative = value - Vertices[0].X;
                Vertices[0].X = value;
                Vertices[1].X += relative;
                Vertices[2].X += relative;
            }
        }
        public float Y
        {
            get => Vertices[0].Y;
            set
            {
                float relative = value - Vertices[0].Y;
                Vertices[0].Y = value;
                Vertices[1].Y += relative;
                Vertices[2].Y += relative;
            }
        }
        public float Z
        {
            get => Vertices[0].Z;
            set
            {
                float relative = value - Vertices[0].Z;
                Vertices[0].Z = value;
                Vertices[1].Z += relative;
                Vertices[2].Z += relative;
            }
        }
        public Vector3 PosA => Vertices[1];
        public Vector3 PosB => Vertices[2];

        public Vector3 DirA
        {
            get => Vertices[1] - Vertices[0];
            set => Vertices[1] = Vertices[0] + value;
        }
        public Vector3 DirB
        {
            get => Vertices[2] - Vertices[0];
            set => Vertices[2] = Vertices[0] + value;
        }

        public Vector3 N => Vector3.Normalize(Vector3.Cross(DirB, DirA));

        public Type GetCollisionType() => typeof(Triangle3);

        public virtual object Clone()
        {
            return new Triangle3(this);
        }

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                Vector3.Min(this[0], Vector3.Min(this[1], this[2])),
                Vector3.Max(this[0], Vector3.Max(this[1], this[2])));
        }

        public virtual Vector3 this[int index] { get => Vertices[index]; set => Vertices[index] = value; }

        public Plane3 ToPlane()
        {
            return new Plane3(Pos, Vector3.Normalize(Vector3.Cross(DirA, DirB)));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Vertices);
        }

        public void Transform(Matrix transform)
        {
            Vertices[0] = Vector3.Transform(Vertices[0], transform);
            Vertices[1] = Vector3.Transform(Vertices[1], transform);
            Vertices[2] = Vector3.Transform(Vertices[2], transform);
        }
    }
}
