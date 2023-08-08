namespace BytingLib
{
    public record struct TextToTextureKey(string Text, SpriteFont Font, Color Color, float Scale)
    {
        public static implicit operator (string, SpriteFont, Color, float)(TextToTextureKey value)
        {
            return (value.Text, value.Font, value.Color, value.Scale);
        }

        public static implicit operator TextToTextureKey((string, SpriteFont, Color, float) value)
        {
            return new TextToTextureKey(value.Item1, value.Item2, value.Item3, value.Item4);
        }
    }
}
