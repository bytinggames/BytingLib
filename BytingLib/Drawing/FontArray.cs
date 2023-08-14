namespace BytingLib
{
    public class FontArray
    {
        private readonly (float, Ref<SpriteFont>)[] fonts;

        /// <summary>floats must be in ascending order (1, 2, 3, etc. for example). Each float represents the maximum font size that the corresponding font supports. The last float is ignored, it has no max.</summary>
        public FontArray((float, Ref<SpriteFont>)[] fonts)
        {
            if (fonts.Length == 0)
                throw new ArgumentException("fonts must be more than 0");

            this.fonts = fonts;
        }

        public Ref<SpriteFont> GetFont(float fontSize)
        {
            for (int i = 0; i < fonts.Length - 1; i++) // skip last font, that is the default one
            {
                if (fontSize <= fonts[i].Item1)
                    return fonts[i].Item2;
            }
            return fonts[fonts.Length - 1].Item2;
        }

        public Ref<SpriteFont> GetFont(float fontSize, out float actualFontSize)
        {
            for (int i = 0; i < fonts.Length - 1; i++) // skip last font, that is the default one
            {
                if (fonts[i].Item1 <= fontSize)
                {
                    actualFontSize = fonts[i].Item1;
                    return fonts[i].Item2;
                }
            }
            actualFontSize = fonts[fonts.Length - 1].Item1;
            return fonts[fonts.Length - 1].Item2;
        }
    }
}
