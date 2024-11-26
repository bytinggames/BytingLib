namespace BytingLib.UI
{
    public class PanelCut : Panel
    {
        public PanelCut(float width = -1f, float height = -1f, Color? color = null, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, color, anchor, padding)
        {
        }

        public override void Update(ElementInput input)
        {
            bool mouseInside = AbsoluteRect.CollidesWith(input.Mouse.Position);
            if (!mouseInside)
            {
                input.DoWhileHoverOutsideOfScissorRect(() => base.Update(input));
            }
            else
            {
                base.Update(input);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, StyleRoot style)
        {
            style.ScissorRect(spriteBatch, AbsoluteRect, () =>
            {
                base.Draw(spriteBatch, style);
            });
        }
    }
}
