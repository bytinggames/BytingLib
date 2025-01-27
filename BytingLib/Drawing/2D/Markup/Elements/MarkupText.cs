namespace BytingLib.Markup
{
    public class MarkupText : MarkupBlock
    {
        public string Text { get; set; }
        public override bool ConfinesToLineSpacing => true;

        public MarkupText(ScriptReaderLiteral reader)
        {
            Text = reader.ReadToCharOrEnd(out char? until, '#', '\n');

            if (until != null)
            {
                reader.Move(-1);
            }
        }

        public MarkupText(string text)
        {
            Text = text;
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings, int start, int end)
        {
            Vector2 size = settings.Font.Value.MeasureString(end == -1 ? Text.Substring(start) : Text.Substring(start, end - start)).GetCeil();
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
                settings.TextUnderline, settings.TextOutline, settings.RoundPositionTo);
        }

        public override string ToString()
        {
            return Text;
        }

        public MarkupText CloneMarkupText(bool removeSubContainer)
        {
            MarkupText clone = (MarkupText)this.MemberwiseClone();

            if (removeSubContainer)
            {
                clone.subContainer = null;
            }

            return clone;
        }
    }
}
