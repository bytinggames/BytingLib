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

    public class Element
    {
        public Element? Parent { get; private set; }
        public List<Element> Children { get; } = new List<Element>();
        /// <summary>negative means take remaining space or %, depending on the parent. 0 means fit to content.</summary>
        public float Width { get; set; } = -1;
        /// <summary>negative means take remaining space or %, depending on the parent. 0 means fit to content.</summary>
        public float Height { get; set; } = -1;
        public Padding? Padding { get; set; }
        public Vector2 Anchor { get; set; } = new Vector2(0.5f);

        public Rect absoluteRect { get; protected set; }

        protected virtual void DrawSelf(SpriteBatch spriteBatch, Style style) { }
        protected virtual void UpdateSelf(ElementInput input) { }

        public virtual void Draw(SpriteBatch spriteBatch, Style style)
        {
            DrawSelf(spriteBatch, style);

            for (int i = 0; i < Children.Count; i++)
                Children[i].Draw(spriteBatch, style);
        }
        public virtual void Update(ElementInput input)
        {
            for (int i = 0; i < Children.Count; i++)
                Children[i].Update(input);

            // call update self after children, cause children are drown on top of parent
            UpdateSelf(input);
        }

        protected virtual void UpdateTreeModifyRect(Rect rect) { }

        public virtual void UpdateTree(Rect rect)
        {
            absoluteRect = rect.CloneRect().Round();

            rect = rect.CloneRect();
            Padding?.RemoveFromRect(rect);
            UpdateTreeModifyRect(rect);

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                Rect childRect = GetChildRect(rect, c);
                Children[i].UpdateTree(childRect);
            }
        }

        protected static Rect GetChildRect(Rect rect, Element c)
        {
            Rect myRect;
            if (c.Width == -1 && c.Height == -1) // shortcut. no need to calculate that stuff in else
            {
                myRect = rect;
            }
            else
            {
                Vector2 size = Vector2.Zero;

                if (c.Width == 0f)
                {
                    if (c.Children.Count > 0)
                        size.X = c.GetWidthTopToBottom();
                }
                else
                    size.X = c.Width > 0 ? c.Width : -c.Width * rect.Width;
                if (c.Height == 0f)
                {
                    if (c.Children.Count > 0)
                        size.Y = c.GetHeightTopToBottom();
                }
                else
                    size.Y = c.Height > 0 ? c.Height : -c.Height * rect.Height;

                Vector2 pos = c.Anchor * rect.Size + rect.Pos;

                myRect = new Anchor(pos, c.Anchor).Rectangle(size);

            }

            return myRect;
        }

        //protected static Rect GetChildRect(Vector2 pos, Vector2 fieldSize, Element c)
        //{
        //    float width = c.Width >= 0 ? c.Width : -c.Width * fieldSize.X;
        //    float height = c.Height >= 0 ? c.Height : -c.Height * fieldSize.Y;
        //    Vector2 remainingSpace = fieldSize - new Vector2(width, height);
        //    Rect r = new Rect(pos + remainingSpace * c.Anchor, new Vector2(width, height));
        //    return r;
        //}

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

        public float GetWidthTopToBottom()
        {
            if (Width < 0)
                return -1;
            if (Width > 0)
                return Width;
            if (Children.Count == 0)
                return 0f;
            return Children.Max(f => f.GetWidthTopToBottom()) + (Padding == null ? 0f : Padding.Width);
        }
        public float GetHeightTopToBottom()
        {
            if (Height < 0)
                return -1;
            if (Height > 0)
                return Height;
            if (Children.Count == 0)
                return 0f;
            return Children.Max(f => f.GetHeightTopToBottom()) + (Padding == null ? 0f : Padding.Height);
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
