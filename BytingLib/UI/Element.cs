namespace BytingLib.UI
{
    public abstract class Element
    {
        public Element? Parent { get; private set; }
        public List<Element> Children { get; } = new List<Element>();
        /// <summary>negative means take remaining space or %, depending on the parent</summary>
        public float Width { get; set; } = -1;
        /// <summary>negative means take remaining space or %, depending on the parent</summary>
        public float Height { get; set; } = -1;
        public Sides? Padding { get; set; }
        public Vector2 Anchor { get; set; } = new Vector2(0.5f);

        public abstract void Draw(SpriteBatch spriteBatch, Rect parentRect);

        public float GetWidth()
        {
            if (Width >= 0)
                return Width - GetPaddingSize().X;
            if (Parent != null)
                return -Width * Parent.GetWidth() - GetPaddingSize().X;
            throw new BytingException("Width can't be null, when there is no parent");
        }

        public float GetHeight()
        {
            if (Height >= 0)
                return Height - GetPaddingSize().Y;
            if (Parent != null)
                return -Height * Parent.GetHeight() - GetPaddingSize().Y;
            throw new BytingException("Height can't be null, when there is no parent");
        }

        public Element Add(params Element[] children)
        {
            for (int i = 0; i < children.Length; i++)
            {
                Children.Add(children[i]);
                children[i].Parent = this;
            }
            return this;
        }

        public Element Remove(params Element[] children)
        {
            for (int i = 0; i < children.Length; i++)
            {
                Children.Remove(children[i]);
                children[i].Parent = null;
            }
            return this;
        }

        public Vector2 GetPaddingSize() => Padding == null ? Vector2.Zero : Padding.GetSize();
    }
}
