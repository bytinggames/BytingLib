
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IShape : IBoundingRectangle, ICloneable
    {
        Vector2 Pos { get; set; }
        float X { get; set; }
        float Y { get; set; }

        void Draw(SpriteBatch spriteBatch, Color color, float depth = 0f);
    }

    public static class IShapeExtension
    {
        public static Vector2 GetCenter(this IShape shape)
        {
            return shape.GetBoundingRectangle().GetCenter();
        }
        public static bool CollidesWith(this IShape myShape, IShape shape) => Collision.GetCollision(myShape, shape);
        public static CollisionResult DistanceTo(this IShape myShape, IShape shape, Vector2 dir) => Collision.GetDistance(myShape, shape, dir);

    }
}
