namespace BytingLib.UI
{
    public class PanelAspectRatio : Element
    {
        public float AspectRatio { get; }
        public Color? Color { get; set; }

        public PanelAspectRatio(float aspectRatio = 1f, Color? color = null, Vector2? anchor = null, Padding? padding = null)
        {
            Width = -1f;
            Height = -1f;
            AspectRatio = aspectRatio;
            Color = color;
            if (anchor != null)
            {
                Anchor = anchor.Value;
            }

            Padding = padding;
        }

        protected override void UpdateTreeInner(Rect rect)
        {
            rect = rect.CloneRect();
            rect.ShrinkToAspectRatio(AspectRatio, Anchor);

            base.UpdateTreeInner(rect);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color.HasValue)
            {
                AbsoluteRect.Draw(spriteBatch, Color.Value);
            }
        }
    }
}
