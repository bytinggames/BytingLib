
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public class Circle : IShape
    {
        private Vector2 pos;

        public Vector2 Pos { get => pos; set => pos = value; }
        public float X { get => pos.X; set => pos.X = value; }
        public float Y { get => pos.Y; set => pos.Y = value; }
        public float Radius { get; set; }

        public Circle(Vector2 pos, float radius)
        {
            this.pos = pos;
            this.Radius = radius;
        }

        public Rect GetBoundingRectangle()
        {
            return new Rect(pos - new Vector2(Radius), new Vector2(Radius * 2));
        }

        public CollisionResult DistanceTo(IShape shape, Vector2 dir)
        {
            throw new NotImplementedException();
        }

        public bool CollidesWith(IShape shape)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return new Circle(pos, Radius);
        }
    }

}
