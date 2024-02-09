﻿namespace BytingLib.Markup
{
    [MarkupShortcut("cursor")]
    public class MarkupCursor : MarkupBlock
    {
        public Color Color { get; set; } = Color.Black;

        public override string ToString()
        {
            return "cursor";
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            return Vector2.Zero;
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            if (settings.TotalMilliseconds % 1000 < 500)
            {
                float lineSpacing = settings.Font.Value.LineSpacing * settings.Scale.Y;
                settings.SpriteBatch.DrawRectangle(settings.Anchor.Rectangle(Math.Max(1, lineSpacing / 12), lineSpacing), Color);
            }
        }
    }
}
