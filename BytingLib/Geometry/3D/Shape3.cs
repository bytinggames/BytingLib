
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public interface IBoundingBox
    {
        public BoundingBox GetBoundingBox();
    }
    public interface IShape3 : IBoundingBox
    {
        public Vector3 Pos { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public CollisionResult DistanceTo(IShape3 shape, Vector3 dir);

        public bool CollidesWith(IShape3 shape);
    }

    public class AABB3
    {
    }

    public class Axis3
    {
    }

    public class Box3
    {
    }

    public class Capsule3
    {
    }

    public class Cylinder3
    {
    }

    public class Line3
    {
    }

    public class Plane3
    {
    }

    public class Ray3
    {
    }

    public class Triangle3
    {
    }

    public class Shape3Collection
    {
    }

    public class Sphere3
    {
    }

    public class Point3
    {
    }
}
