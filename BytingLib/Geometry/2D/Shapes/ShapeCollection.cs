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

        public ShapeCollection(params IShape[] shapes)
        {
            if (shapes.Length == 0)
                throw new BytingException("ShapeCollection contains 0 shapes. That should never happen.");

            Shapes = shapes;
        }

        public ShapeCollection(Vector2 pos, params IShape[] shapes)
        {
            if (shapes.Length == 0)
                throw new BytingException("ShapeCollection contains 0 shapes. That should never happen.");

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
                throw new BytingException("ShapeCollection contains 0 shapes. That should never happen.");
            return Rect.FromRects(Shapes.Select(f => f.GetBoundingRect()))!;
        }
    }
}
