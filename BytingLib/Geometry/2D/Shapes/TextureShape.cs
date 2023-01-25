namespace BytingLib
{
    /// <summary>
    /// Not implemented yet
    /// </summary>
    public class TextureShape : IShape
    {
        public Vector2 Pos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float X { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Y { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Int2 Size { get; set; }
        public Color[] colorData;

        public TextureShape()
        {
            colorData = new Color[0];
        }

        public Type GetCollisionType() => typeof(TextureShape);

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        public bool CollidesWith(IShape shape)
        {
            throw new NotImplementedException();
        }

        public CollisionResult DistanceTo(IShape shape, Vector2 dir)
        {
            throw new NotImplementedException();
        }

        public Rect GetBoundingRect()
        {
            throw new NotImplementedException();
        }

        internal Matrix GetMatrix()
        {
            throw new NotImplementedException();
        }

        internal bool IsTransformed()
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float depth = 0)
        {
            throw new NotImplementedException();
        }
    }
}
