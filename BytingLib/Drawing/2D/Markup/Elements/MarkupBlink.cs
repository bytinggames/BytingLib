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
            bool visible = false;
            if (!show)
            {
                visible = settings.Visible;
                settings.Visible = false;
            }

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }
            if (!show)
            {
                settings.Visible = visible;
            }
        }
    }
}
