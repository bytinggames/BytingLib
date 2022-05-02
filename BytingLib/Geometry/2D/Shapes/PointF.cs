using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class PointF : IShape
    {
        private Vector2 pos;

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }

        public PointF(Vector2 pos)
        {
            this.pos = pos;
        }
        public PointF(float x, float y)
        {
            pos = new Vector2(x,y);
        }
        public PointF(Point point)
        {
            pos = point.ToVector2();
        }

        public object Clone()
        {
            return new PointF(pos);
        }

        public bool CollidesWith(IShape shape) => Collision.GetCollision(this, shape);
        public CollisionResult DistanceTo(IShape shape, Vector2 dir) => Collision.GetDistance(this, shape, dir);

        public Rect GetBoundingRectangle()
        {
            return new Rect(pos, Vector2.Zero);
        }
    }

}
