using BytingLib.Markup;

namespace BytingLib
{
    public class TextToTexture : IDisposable
    {
        private readonly SpriteBatch spriteBatch;
        private readonly FontArray fontArray;
        private readonly IShaderColor textEffect; // TODO: convert to shader?
        private readonly Creator markupCreator;
        private readonly float verticalSpaceBetweenLines;
        private readonly Dictionary<TextToTextureKey, AssetHolder<Texture2D>> textures = new();
        private readonly DisposableContainer disposables = new();
        public float MinimumPixelsPerUnit { get; }

        public TextToTexture(SpriteBatch spriteBatch, FontArray fontArray, IShaderColor textEffect, Creator markupCreator, float verticalSpaceBetweenLines, float minimumPixelsPerUnit)
        {
            this.spriteBatch = spriteBatch;
            this.fontArray = fontArray;
            this.textEffect = textEffect;
            this.markupCreator = markupCreator;
            this.verticalSpaceBetweenLines = verticalSpaceBetweenLines;
            MinimumPixelsPerUnit = minimumPixelsPerUnit;
        }

        public Promise<Ref<Texture2D>> UseTexture(string text, Vector3 right, Color backgroundColor, float? verticalSpaceBetweenLines = null)
        {
            Promise<Ref<Texture2D>> tex = new(() =>
            {
                var font = fontArray.GetFont(1f, out float actualFontSize);
                var markupSettings = new MarkupSettings(spriteBatch, font, Anchor.TopLeft(0, 0), Color.White);
                markupSettings.VerticalSpaceBetweenLines = verticalSpaceBetweenLines ?? this.verticalSpaceBetweenLines;
                var drawElement = new MarkupRoot(markupCreator, text);
                Vector2 textSize = drawElement.GetSize(markupSettings);
                textSize /= actualFontSize;

                int fontSize = GetRightFontSize(right.Length() * 2f /* because right only measures half the length */,
                    (int)MathF.Ceiling(textSize.X), MinimumPixelsPerUnit);

                return CreateTextTexture(text, fontArray.GetFont(fontSize), backgroundColor, fontSize, markupSettings.VerticalSpaceBetweenLines * fontSize);
            });
            return tex;
        }

        private static int GetRightFontSize(float spaceInMeters, int textureWidthOfScale1, float targetPixelsPerCM)
        {
            float pixelsPerMeter = textureWidthOfScale1 / spaceInMeters;

            float requiredFontScale = targetPixelsPerCM / pixelsPerMeter;

            int fontSize = (int)MathF.Ceiling(requiredFontScale);
            fontSize = Math.Min(8, Math.Max(1, fontSize));
            return fontSize;
        }

        public Ref<Texture2D> CreateTextTexture(string text, Ref<SpriteFont> font, Color backgroundColor, float textureScale = 1, float? verticalSpaceBetweenLines = null)
        {
            if (textures.ContainsKey((text, font.Value, backgroundColor, textureScale)))
            {
                return textures[(text, font.Value, backgroundColor, textureScale)].Use();
            }

            var markupSettings = new MarkupSettings(spriteBatch, font, Anchor.TopLeft(0, 0), Color.Black /* default text color is black */);
            markupSettings.TextureScale = Vector2.One * textureScale;
            markupSettings.VerticalSpaceBetweenLines = verticalSpaceBetweenLines ?? this.verticalSpaceBetweenLines;
            var drawElement = new MarkupRoot(markupCreator, text);

            Vector2 textSize = drawElement.GetSize(markupSettings);

            RenderTarget2D? tex = null;

            Promise<Texture2D> promise = new(() =>
            {
                var gDevice = spriteBatch.GraphicsDevice;
                tex = new RenderTarget2D(gDevice, (int)Math.Ceiling(textSize.X), (int)Math.Ceiling(textSize.Y), false, SurfaceFormat.Color, DepthFormat.None);

                using (gDevice.UseRenderTarget(tex))
                {
                    gDevice.Clear(Color.Transparent);

                    using (textEffect.Color.Use(backgroundColor.ToVector4()))
                    {
                        textEffect.ApplyParameters();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, textEffect.Effect.Value);

                        drawElement.Draw(markupSettings);

                        spriteBatch.End();
                    }
                }

                //tex.SaveAsPng(@"C:\Users\Julian\Desktop\out\out.png");

                disposables.Use(tex);
                return tex;
            });

            AssetHolder<Texture2D> assetHolder = new AssetHolder<Texture2D>(promise, "TextToTexture_" + text, _ =>
            {
                if (!textures.Remove((text, font.Value, backgroundColor, textureScale)))
                {
                    throw new BytingException("couldn't remove a texture from TextToTexture.textures");
                }

                tex?.Dispose();
            });

            textures.Add((text, font.Value, backgroundColor, textureScale), assetHolder);

            return assetHolder.Use();
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
