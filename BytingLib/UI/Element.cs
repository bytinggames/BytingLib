﻿namespace BytingLib.UI
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

    public class Element : IDisposable
    {
        public Element? Parent { get; private set; }
        public List<Element> Children { get; } = new List<Element>();
        /// <summary>negative means take remaining space or %, depending on the parent. 0 means fit to content.</summary>
        public float Width { get; set; } = -1f;
        /// <summary>negative means take remaining space or %, depending on the parent. 0 means fit to content.</summary>
        public float Height { get; set; } = -1f;
        public Padding? Padding { get; set; }
        public Vector2 Anchor { get; set; } = new Vector2(0.5f);
        /// <summary>Return true, when you don't want parents or siblings get hovered too. Catch the hover event.</summary>
        public event OnWhileHoverDelegate? OnHoverSustain;
        public event Action? OnHoverEnter;
        public event Action? OnHoverExit;
        public delegate bool OnWhileHoverDelegate(Element element, ElementInput input);
        public delegate void ElementAction(Element element);
        /// <summary>When invisible, Update is also not called. Not even for the children.</summary>
        public bool Visible { get; set; } = true;
        public bool IsHoverEnabled => OnHoverSustain != null || OnHoverEnter != null || OnHoverExit != null;
        /// <summary>Wether the mouse hovers over the element. This is only updated, when <see cref="IsHoverEnabled"/> is true.</summary>
        public bool Hover
        {
            get => hover;
            set
            {
                if (value != hover)
                {
                    hover = value;

                    if (value)
                    {
                        OnHoverEnter?.Invoke();
                    }
                    else
                    {
                        OnHoverExit?.Invoke();
                    }
                }
            }
        }

        private bool setChildrenWidthToMaxChildWidth = false;
        private bool setChildrenHeightToMaxChildHeight = false;
        private bool hover;

        public float Size(int dimension)
        {
            return dimension switch
            {
                0 => Width,
                1 => Height,
                _ => throw new BytingException("dimension of " + dimension + " does not exist")
            }; ;
        }

        public void SetSize(int dimension, float size)
        {
            switch (dimension)
            {
                case 0:
                    if (Width != size)
                    {
                        Width = size;
                        SetDirty();
                    }
                    break;
                case 1:
                    if (Height != size)
                    {
                        Height = size;
                        SetDirty();
                    }
                    break;
                default: throw new BytingException("dimension of " + dimension + " does not exist");
            };
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Rect AbsoluteRect { get; protected set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Style? Style { get; set; }

        protected virtual void DrawSelf(SpriteBatch spriteBatch, StyleRoot style) { }
        protected virtual void DrawSelfPost(SpriteBatch spriteBatch, StyleRoot style) { }
        protected virtual void UpdateSelf(ElementInput input)
        {
            UpdateHoverElement(input);
        }

        protected virtual void UpdateHoverElement(ElementInput input)
        {
            if (input.HoverElement == null
                && IsHoverEnabled)
            {
                //bool alreadyHovering = input.HoverElementForTooltip == this;
                Hover = AbsoluteRect.CollidesWith(input.Mouse.Position);
                if (Hover && OnHoverSustain != null)
                {
                    var results = OnHoverSustain.GetInvocationList().Select(x => (bool)x.DynamicInvoke(this, input)!);
                    // if any invocation returned true, catch the hover ability
                    if (results.ToArray() // to array forces all subscribers to get invoked. Otherwise with .Any() it would stop once true is returned.
                        .Any(f => f))
                    {
                        input.HoverElement = this;
                    }
                }
            }
            else if (input.HoverElement != this)
            {
                Hover = false;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (!Visible)
            {
                return;
            }

            PushMyStyle(style);

            DrawSelf(spriteBatch, style);

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch, style);
            }

            DrawSelfPost(spriteBatch, style);

            PopMyStyle(style);
        }

        protected virtual void PushMyStyle(StyleRoot style)
        {
            style.Push(Style);
        }

        protected virtual void PopMyStyle(StyleRoot style)
        {
            style.Pop(Style);
        }

        public virtual void Update(ElementInput input)
        {
            if (!Visible)
            {
                return;
            }

            // start at the last child, as that is the one that gets drawn topmost
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].Update(input);
            }

            // call update self after children, cause children are drawn on top of parent
            UpdateSelf(input);
        }

        protected virtual void UpdateTreeModifyRect(Rect rect) { }

        public void UpdateTreeBegin(StyleRoot style)
        {
            style.Push(Style);

            UpdateTreeBeginSelf(style);

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].UpdateTreeBegin(style);
            }

            style.Pop(Style);
        }

        protected virtual void UpdateTreeBeginSelf(StyleRoot style) { }

        public void UpdateTree(Rect rect)
        {
            if (setChildrenWidthToMaxChildWidth)
            {
                setChildrenWidthToMaxChildWidth = false;
                SetChildrenSizesToMaxChildSize(0, rect.Size);
            }
            if (setChildrenHeightToMaxChildHeight)
            {
                setChildrenHeightToMaxChildHeight = false;
                SetChildrenSizesToMaxChildSize(1, rect.Size);
            }

            UpdateTreeInner(rect);
        }

        protected virtual void UpdateTreeInner(Rect rect)
        {
            AbsoluteRect = rect.CloneRect().Round();

            rect = rect.CloneRect();
            Padding?.RemoveFromRect(rect);
            UpdateTreeModifyRect(rect);

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                Rect childRect = GetChildRect(rect, c);
                c.UpdateTree(childRect);
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
                float[] size = new float[] { c.Width, c.Height };

                for (int d = 0; d < 2; d++)
                {
                    if (size[d] == 0f && c.Children.Count > 0)
                    {
                        size[d] = c.GetSizeTopToBottom(d, rect.Size);
                    }

                    if (size[d] < 0)
                    {
                        size[d] = -size[d] * (d == 0 ? rect.Width : rect.Height);
                    }
                }

                Vector2 pos = c.Anchor * rect.Size + rect.Pos;

                myRect = new Anchor(pos, c.Anchor).Rectangle(size[0], size[1]);

            }

            return myRect;
        }

        public virtual float GetSizeTopToBottom(int d, Vector2 parentContainerSize)
        {
            float size = Size(d);
            if (size == 0)
            {
                float pad = Padding == null ? 0f : Padding.Size(d);
                if (Children.Count == 0)
                {
                    return pad;
                }

                float maxSize = Children.Max(f => f.GetSizeTopToBottom(d, parentContainerSize));
                if (maxSize > 0)
                {
                    return maxSize + pad;
                }
                else // no positive values
                {
                    return Children.Min(f => f.GetSizeTopToBottom(d, parentContainerSize));
                }
            }
            return size;
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
        public Element Add(List<Element> children)
        {
            for (int i = 0; i < children.Count; i++)
            {
                Children.Add(children[i]);
                children[i].Parent = this;
            }
            return this;
        }
        public Element AddMaybeNull(params Element?[] children)
        {
            for (int i = 0; i < children.Length; i++)
            {
                var c = children[i];
                if (c == null)
                {
                    continue;
                }

                Children.Add(c);
                c.Parent = this;
            }
            return this;
        }
        public Element AddEnumerable(IEnumerable<Element> children)
        {
            foreach (var c in children)
            {
                Children.Add(c);
                c.Parent = this;
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
        public Element RemoveEnumerable(IEnumerable<Element> children)
        {
            foreach (var c in children)
            {
                Children.Remove(c);
                c.Parent = null;
            }
            return this;
        }

        public void Clear()
        {
            while (Children.Count > 0)
            {
                Remove(Children[^1]);
            }
        }

        public void InsertBefore(Element beforeElement, Element elementToInsert)
        {
            int index = Children.IndexOf(beforeElement);

            if (index == -1)
            {
                Add(elementToInsert);
            }
            else
            {
                Insert(index, elementToInsert);
            }
        }
        public void InsertAfter(Element afterElement, Element elementToInsert)
        {
            int index = Children.IndexOf(afterElement);

            if (index == -1)
            {
                Add(elementToInsert);
            }
            else
            {
                Insert(index + 1, elementToInsert);
            }
        }

        public void Insert(int index, Element element)
        {
            Children.Insert(index, element);
            element.Parent = this;
        }

        public Element SetStyle(Style style)
        {
            Style = style;
            return this;
        }

        public Vector2 GetPaddingSize() => Padding == null ? Vector2.Zero : Padding.GetSize();

        public virtual void SetDirty()
        {
            Parent?.SetDirty();
        }

        public void Dispose()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Dispose();
            }

            DisposeSelf();
        }

        protected virtual void DisposeSelf()
        {
        }

        /// <summary>
        /// For when the ui loses focus. Buttons for example should loose the hover state then.
        /// </summary>
        public virtual void LooseFocus()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].LooseFocus();
            }
        }

        public Element Tooltip(ITooltip tooltip, string text)
        {
            OnHoverSustain += (f, input) => { tooltip.OnHover(f, text); return false; } ;
            return this;
        }

        public Element Tooltip(ITooltip tooltip, Func<string> getText)
        {
            OnHoverSustain += (f, input) => { tooltip.OnHover(f, getText()); return false; };
            return this;
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public Element SetChildrenWidthToMaxChildWidth()
        {
            setChildrenWidthToMaxChildWidth = true; // SetChildrenSizesToMaxChildSize(0);
            SetDirty();
            return this;
        }
        public Element SetChildrenHeightToMaxChildHeight()
        {
            setChildrenHeightToMaxChildHeight = true;//SetChildrenSizesToMaxChildSize(1);
            SetDirty();
            return this;
        }
        private Element SetChildrenSizesToMaxChildSize(int d, Vector2 parentContainerSize)
        {
            float maxSize = 0f;

            for (int i = 0; i < Children.Count; i++)
            {
                float size = Children[i].GetSizeTopToBottom(d, parentContainerSize);
                if (size >= 0f)
                {
                    if (size > maxSize)
                    {
                        maxSize = size;
                    }
                }
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].SetSize(d, maxSize);
            }

            return this;
        }
    }
}
