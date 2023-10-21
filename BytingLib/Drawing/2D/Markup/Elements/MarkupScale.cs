namespace BytingLib.Markup
{
    [MarkupShortcut("scale")]
    public class MarkupScale : MarkupCollection
    {
        float scale;

        public MarkupScale(Creator creator, float scale, string text)
            :base(creator, text)
        {
            this.scale = scale;
        }

        public override string ToString()
        {
            return $"scale: {scale} {base.ToString()}";
        }

        public override IEnumerable<ILeaf> IterateOverLeaves(MarkupSettings settings)
        {
            Vector2 temp = settings.Scale;
            settings.Scale = new Vector2(scale);

            foreach (var leaf in base.IterateOverLeaves(settings))
            {
                yield return leaf;
            }

            settings.Scale = temp;
        }
    }
}
