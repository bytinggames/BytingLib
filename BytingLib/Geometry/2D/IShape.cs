
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public interface IShape : IBoundingRectangle, ICloneable
    {
        public Vector2 Pos { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public CollisionResult DistanceTo(IShape shape, Vector2 dir);

        public bool CollidesWith(IShape shape);
    }
}
