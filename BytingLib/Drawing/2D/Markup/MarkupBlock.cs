namespace BytingLib.Markup
{
    public abstract class MarkupBlock : ILeaf
    {
        public float MarginRight { get; set; }
        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }

        // for sub element
        /// <summary>No PaddingRight support for SubSizeUnion yet.</summary>
        public float PaddingRight { get; set; }
        /// <summary>No PaddingLeft support for SubSizeUnion yet.</summary>
        public float PaddingLeft { get; set; }
        /// <summary>No PaddingTop support for SubSizeUnion yet.</summary>
        public float PaddingTop { get; set; }
        /// <summary>No PaddingBottom support for SubSizeUnion yet.</summary>
        public float PaddingBottom { get; set; }

        protected List<MarkupRoot>? subContainers;

        public bool SubSizeUnion { get; set; } = false;

        public float SubAnchorX { get; set; } = 0.5f;
        /// <summary>No SubAnchorY support for SubSizeUnion yet.</summary>
        public float SubAnchorY { get; set; } = 0.5f;

        public abstract bool ConfinesToLineSpacing { get; }

        public void Sub(Creator creator, string text)
        {
            if (subContainers == null)
            {
                subContainers = new();
            }

            subContainers.Add(new MarkupRoot(creator, text));
        }

        public void SubAnchor(float x, float y)
        {
            SubAnchorX = x;
            SubAnchorY = y;
        }


        public void Draw(MarkupSettings settings)
        {
            if (!settings.Visible)
            {
                return;
            }

            if (MarginLeft == 0 && MarginTop == 0 && MarginRight == 0 && MarginBottom == 0)
            {
                InnerDraw(settings);
            }
            else
            {
                Vector2 temp = settings.Anchor.Pos;
                settings.Anchor.Pos.X += (MarginLeft * (1f - settings.Anchor.OX) - MarginRight * settings.Anchor.OX) * settings.Scale.X;
                settings.Anchor.Pos.Y += (MarginTop * (1f - settings.Anchor.OY) - MarginBottom * settings.Anchor.OY) * settings.Scale.Y;
                InnerDraw(settings);
                settings.Anchor.Pos = temp;
            }
        }
        private void InnerDraw(MarkupSettings settings)
        {
            float tempX = settings.Anchor.X;
            if (SubSizeUnion && subContainers != null && subContainers.Count > 0)
            {
                Vector2 thisSize = GetSizeChild(settings, 0, -1);
                Vector2 subSize = subContainers.Max(f => f.GetSize(settings));
                Vector2 larger = subSize - thisSize;
                if (larger.X > 0)
                {
                    settings.Anchor.X += larger.X * SubAnchorX;
                }
            }

            DrawChild(settings);

            if (subContainers != null && subContainers.Count > 0)
            {
                var settingsClone = settings.CloneMarkupSettings();
                Rect ownRect = settings.Anchor.Rectangle(GetSizeChild(settingsClone, 0, -1));
                ownRect.ApplyPadding(PaddingLeft * settings.Scale.X, PaddingRight * settings.Scale.X, PaddingTop * settings.Scale.Y, PaddingBottom * settings.Scale.Y);
                settingsClone.Anchor = ownRect.GetAnchor(SubAnchorX, SubAnchorY);
                for (int i = 0; i < subContainers.Count; i++)
                {
                    subContainers[i].Draw(settingsClone);
                }
            }

            settings.Anchor.X = tempX;
        }

        protected abstract void DrawChild(MarkupSettings settings);

        public Vector2 GetSize(MarkupSettings settings, int start, int end)
        {
            Vector2 size = GetSizeChild(settings, start, end);

            if (SubSizeUnion && subContainers != null && subContainers.Count > 0)
            {
                Vector2 subSize = subContainers.Max(f => f.GetSize(settings));
                size = Vector2.Max(size, subSize);
            }

            size.X += (MarginRight + MarginLeft) * settings.Scale.X;
            size.Y += (MarginTop + MarginBottom) * settings.Scale.Y;
            return size;
        }

        protected Vector2 GetSizeChild(MarkupSettings settings, int start, int end) => GetSizeChildUnscaled(settings, start, end) * settings.Scale;

        protected abstract Vector2 GetSizeChildUnscaled(MarkupSettings settings, int start, int end);

        public void MarginTopBottom(float val)
        {
            MarginTop = val;
            MarginBottom = val;
        }
        public void MarginLeftRight(float val)
        {
            MarginLeft = val;
            MarginRight = val;
        }
        public void Margin(float val)
        {
            MarginTop = val;
            MarginBottom = val;
            MarginLeft = val;
            MarginRight = val;
        }

        public virtual void Dispose()
        {
        }
    }
}
