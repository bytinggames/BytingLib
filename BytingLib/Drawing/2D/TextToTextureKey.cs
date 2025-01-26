namespace BytingLib
{
    public record struct TextToTextureKey(string Text, SpriteFont Font, Color Color, Vector2 TextureScale)
    {
        public static implicit operator (string, SpriteFont, Color, Vector2)(TextToTextureKey value)
        {
            return (value.Text, value.Font, value.Color, value.TextureScale);
        }

        public static implicit operator TextToTextureKey((string, SpriteFont, Color, Vector2) value)
        {
            return new TextToTextureKey(value.Item1, value.Item2, value.Item3, value.Item4);
        }
    }
}
