namespace BytingLib.Markup
{
    [MarkupShortcut("blink")]
    public class MarkupBlink : MarkupCollection
    {
        private readonly float intervalSeconds;

        public MarkupBlink(Creator creator, float intervalSeconds, string text)
            :base(creator, text)
        {
            this.intervalSeconds = intervalSeconds;
        }

        public override string ToString()
        {
            return $"#blink {base.ToString()}";
        }

        public override IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            bool show = settings.TotalMilliseconds / 1000 % intervalSeconds < intervalSeconds / 2;
            Color? tempTexColor = null;
            Color? tempFontColor = null;
            if (!show)
            {
                tempTexColor = settings.TextureColor;
                tempFontColor = settings.TextColor;
                settings.TextureColor = settings.TextColor = Color.Transparent;
            }

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }
            if (tempTexColor != null && tempFontColor != null)
            {
                settings.TextureColor = tempTexColor.Value;
                settings.TextColor = tempFontColor.Value;
            }
        }
    }
}
