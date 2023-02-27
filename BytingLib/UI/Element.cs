namespace BytingLib.UI
{
    struct ElementIteration
    {
        public Element Element { get; set; }
        public Rect Rect { get; set; }

        public ElementIteration(Element element, Rect rect)
        {
            Element = element;
            Rect = rect;
        }
    }

    public abstract class Element
    {
        public Element? Parent { get; private set; }
        public List<Element> Children { get; } = new List<Element>();
        /// <summary>negative means take remaining space or %, depending on the parent</summary>
        public float Width { get; set; } = -1;
        /// <summary>negative means take remaining space or %, depending on the parent</summary>
        public float Height { get; set; } = -1;
        public Padding? Padding { get; set; }
        public Vector2 Anchor { get; set; } = new Vector2(0.5f);

        public Rect absoluteRect { get; protected set; }

        protected abstract void DrawSelf(SpriteBatch spriteBatch);
        protected virtual void UpdateSelf(ElementInput input) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            DrawSelf(spriteBatch);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Draw(spriteBatch);
        }
        public virtual void Update(ElementInput input)
        {
            for (int i = 0; i < Children.Count; i++)
                Children[i].Update(input);

            // call update self after children, cause children are drown on top of parent
            UpdateSelf(input);
        }

        public virtual void UpdateTree(Rect rect)
        {
            absoluteRect = rect.CloneRect();

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                Vector2 size = new Vector2(c.Width >= 0 ? c.Width : -c.Width * rect.Width, c.Height >= 0 ? c.Height : -c.Height * rect.Height);
                Vector2 pos = c.Anchor * rect.Size + rect.Pos;

                Rect myRect = new Anchor(pos, c.Anchor).Rectangle(size);

                Children[i].UpdateTree(myRect);
            }
        }

        public float GetInnerWidth()
        {
            if (Width >= 0)
                return Width - GetPaddingSize().X;
            if (Parent != null)
                return -Width * Parent.GetInnerWidth() - GetPaddingSize().X;
            throw new BytingException("Width can't be null, when there is no parent");
        }

        public float GetInnerHeight()
        {
            if (Height >= 0)
                return Height - GetPaddingSize().Y;
            if (Parent != null)
                return -Height * Parent.GetInnerHeight() - GetPaddingSize().Y;
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
