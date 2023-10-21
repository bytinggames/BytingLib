namespace BytingLib.Markup
{
    [MarkupShortcut("underline")]
    public class MarkupUnderline : MarkupCollection
    {
        Color? color; // if null, takes color of text
        float thickness;
        float offset;

        public bool SizeUnion { get; set; }

        public MarkupUnderline(Creator creator, string text)
            : base(creator, text)
        {
            thickness = 1f;
        }
        public MarkupUnderline(Creator creator, string hexColor, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
            thickness = 1f;
        }
        public MarkupUnderline(Creator creator, string hexColor, float thickness, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
            this.thickness = thickness;
        }
        public MarkupUnderline(Creator creator, string hexColor, float thickness, float offset, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
            this.thickness = thickness;
            this.offset = offset;
        }

        public override string ToString()
        {
            return $"Underline {base.ToString()}";
        }

        public override IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            var temp = settings.TextUnderline?.CloneUnderline();
            settings.TextUnderline = new MarkupSettings.Underline(color ?? settings.TextColor, thickness, SizeUnion, offset);

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }

            settings.TextUnderline = temp;
        }
    }
}
