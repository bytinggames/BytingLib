namespace BytingLib.Markup
{
    [MarkupShortcut("c")]
    public class MarkupColor : MarkupCollection
    {
        Color textColor;

        public MarkupColor(Creator creator, string hexColor, string text)
            :base(creator, text)
        {
            textColor = ColorExtension.FromHex(hexColor);
        }

        public override string ToString()
        {
            return $"#{textColor.ToHex()} {base.ToString()}";
        }

        public override IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            Color temp = settings.TextColor;
            settings.TextColor = textColor;

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }

            settings.TextColor = temp;
        }
    }
}
