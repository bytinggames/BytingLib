using Microsoft.Xna.Framework;

namespace BytingLib.Markup
{
    public class MarkupText : MarkupBlock
    {
        public string Text { get; }

        public MarkupText(ScriptReader reader)
        {
            Text = reader.ReadToCharOrEnd(out char? until, '#', '\n');

            if (until != null)
                reader.Move(-1);
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            Vector2 size = settings.Font.Value.MeasureString(Text).GetCeil();
            if (settings.TextOutline != null
                && settings.TextOutline.SizeUnion)
            {
                size.X += settings.TextOutline.Thickness * 2f * settings.Scale.X;
                // modifying size.Y would mess with vertical positioning (if top aligned f.ex.)
            }
            return size;
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            if (settings.TextOutline != null && settings.TextOutline.SizeUnion)
            {
                float xTemp = settings.Anchor.X;
                settings.Anchor.X += settings.TextOutline.Thickness * settings.Scale.X;
                DrawChildInner(settings);
                settings.Anchor.X = xTemp;
            }
            else
            {
                DrawChildInner(settings);
            }
        }

        private void DrawChildInner(MarkupSettings settings)
        {
            settings.Font.Value.Draw(settings.SpriteBatch, Text, settings.Anchor, settings.TextColor, settings.Scale, settings.Rotation, settings.Effects,
                settings.TextUnderline, settings.TextOutline);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
