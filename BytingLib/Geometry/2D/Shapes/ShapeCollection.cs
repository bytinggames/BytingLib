using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    /// <summary>
    /// only changes to the position are applied to children of this collection. -> the children move with the parent
    /// </summary>
    public class ShapeCollection : IShape
    {
        private Vector2 pos;

        public Vector2 Pos { get => pos; set => Move(value - pos); }
        public float X { get => pos.X; set => Move(new Vector2(value - pos.X, 0f)); }
        public float Y { get => pos.Y; set => Move(new Vector2(0f, value - pos.X)); }

        public IList<IShape> Shapes { get; set; }

        public ShapeCollection()
        {
            Shapes = new List<IShape>();
        }

        public ShapeCollection(params IShape[] shapes)
        {
            Shapes = shapes;
        }

        public ShapeCollection(Vector2 pos, params IShape[] shapes)
        {
            Shapes = shapes;
            Move(pos);
        }

        private void Move(Vector2 move)
        {
            if (move == Vector2.Zero)
                return;

            for (int i = 0; i < Shapes.Count; i++)
                Shapes[i].Pos += move;
            pos += move;
        }

        public Type GetCollisionType() => typeof(ShapeCollection);

        public virtual object Clone()
        {
            ShapeCollection clone = (ShapeCollection)MemberwiseClone();
            clone.Shapes = Shapes.ToList();
            for (int i = 0; i < clone.Shapes.Count; i++)
            {
                clone.Shapes[i] = (IShape)Shapes[i].Clone();
            }
            return clone;
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float depth = 0)
        {
            for (int i = 0; i < Shapes.Count; i++)
                Shapes[i].Draw(spriteBatch, color, depth);
        }

        public Rect GetBoundingRect()
        {
            if (Shapes.Count == 0)
                return new Rect(float.NaN, float.NaN, float.NaN, float.NaN); // TODO: not sure if that is a good idea...? but what is the alternative?
            return Rect.FromRects(Shapes.Select(f => f.GetBoundingRect()))!;
        }
    }
}
