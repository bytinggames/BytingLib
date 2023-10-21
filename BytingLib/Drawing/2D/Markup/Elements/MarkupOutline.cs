namespace BytingLib.Markup
{
    [MarkupShortcut("outline")]
    public class MarkupOutline : MarkupCollection
    {
        Color? color; // if null, takes color of text
        float thickness = 1f;
        /// <summary>Negative for only bottom outline (used for drawing over the underline).</summary>
        int quality = 4;

        public bool SizeUnion { get; set; }

        public MarkupOutline(Creator creator, string text)
            : base(creator, text)
        { }
        public MarkupOutline(Creator creator, string hexColor, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
        }
        public MarkupOutline(Creator creator, string hexColor, float thickness, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
            this.thickness = thickness;
        }

        public MarkupOutline(Creator creator, string hexColor, float thickness, int quality, string text)
            : base(creator, text)
        {
            color = ColorExtension.FromHex(hexColor);
            this.thickness = thickness;
            this.quality = quality;
        }

        public override string ToString()
        {
            return $"Outline #{color?.ToHex()} {base.ToString()}";
        }

        public override IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            var temp = settings.TextOutline?.CloneOutline();
            settings.TextOutline = new MarkupSettings.Outline(color ?? settings.TextColor, thickness, SizeUnion, quality);

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }

            settings.TextOutline = temp;
        }
    }
}
