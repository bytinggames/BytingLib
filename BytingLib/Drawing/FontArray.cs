namespace BytingLib
{
    public class FontArray
    {
        public (float, Ref<SpriteFont>)[] Fonts { get; }

        /// <summary>floats must be in ascending order (1, 2, 3, etc. for example). Each float represents the maximum font size that the corresponding font supports. The last float is ignored, it has no max.</summary>
        public FontArray((float, Ref<SpriteFont>)[] fonts)
        {
            if (fonts.Length == 0)
            {
                throw new ArgumentException("fonts must be more than 0");
            }

            this.Fonts = fonts;
        }

        public Ref<SpriteFont> GetFont(float fontSize)
        {
            for (int i = 0; i < Fonts.Length - 1; i++) // skip last font, that is the default one
            {
                if (fontSize <= Fonts[i].Item1)
                {
                    return Fonts[i].Item2;
                }
            }
            return Fonts[Fonts.Length - 1].Item2;
        }

        public Ref<SpriteFont> GetFont(float fontSize, out float actualFontSize)
        {
            for (int i = 0; i < Fonts.Length - 1; i++) // skip last font, that is the default one
            {
                if (Fonts[i].Item1 <= fontSize)
                {
                    actualFontSize = Fonts[i].Item1;
                    return Fonts[i].Item2;
                }
            }
            actualFontSize = Fonts[Fonts.Length - 1].Item1;
            return Fonts[Fonts.Length - 1].Item2;
        }
    }
}
