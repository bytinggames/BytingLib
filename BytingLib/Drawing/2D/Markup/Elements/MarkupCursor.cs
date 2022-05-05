using BytingLib.Creation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BytingLib.Markup
{
    [MarkupShortcut("cursor")]
    public class MarkupCursor : MarkupBlock
    {

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
            if (settings.Time % 60 < 30)
            settings.SpriteBatch.DrawRectangle(settings.Anchor.Rectangle(Math.Max(1, settings.Font.Value.LineSpacing / 12), settings.Font.Value.LineSpacing), Color.Black);
        }
    }
}
