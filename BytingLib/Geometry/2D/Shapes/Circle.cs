
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public object Clone()
        {
            return new Circle(pos, Radius);
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float depth = 0f)
        {
            spriteBatch.DrawCircle(this, color, depth);
        }

        public Polygon ToPolygon(int vertexCount) => Polygon.GetCircle(Pos, Radius, vertexCount);

        public PrimitiveLineRing Outline(int vertexCount) => new PrimitiveLineRing(this, vertexCount);
    }

}
