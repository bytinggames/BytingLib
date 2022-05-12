
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IShape : IBoundingRect, ICloneable
    {
        Vector2 Pos { get; set; }
        float X { get; set; }
        float Y { get; set; }

        void Draw(SpriteBatch spriteBatch, Color color, float depth);
    }

    public static class IShapeExtension
    {
        public static Vector2 GetCenter(this IShape shape)
        {
            return shape.GetBoundingRect().GetCenter();
        }
        public static bool CollidesWith(this IShape myShape, IShape shape) => Collision.GetCollision(myShape, shape);
        public static bool CollidesWith(this IShape myShape, Vector2 vec) => Collision.GetCollision(myShape, vec);
        public static CollisionResult DistanceTo(this IShape myShape, IShape shape, Vector2 dir) => Collision.GetDistance(myShape, shape, dir);

        public static void Draw(this IShape shape, SpriteBatch spriteBatch, Color color)
            => shape.Draw(spriteBatch, color, spriteBatch.DefaultDepth);
    }
}
