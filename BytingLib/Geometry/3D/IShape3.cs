namespace BytingLib
{
    public interface IShape3 : IBoundingBox, ICloneable
    {
        public Vector3 Pos { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Type GetCollisionType();
    }

    public static class IShape3Extension
    {
        public static Vector3 GetCenter(this IShape3 shape)
        {
            return shape.GetBoundingBox().GetCenter();
        }
        public static bool CollidesWith(this IShape3 myShape, IShape3 shape) => Collision3.GetCollision(myShape, shape);
        public static bool CollidesWith(this IShape3 myShape, Vector3 vec) => Collision3.GetCollision(myShape, vec);
        public static CollisionResult3 DistanceTo(this IShape3 myShape, IShape3 shape, Vector3 dir) => Collision3.GetDistance(myShape, shape, dir);
    }
}
