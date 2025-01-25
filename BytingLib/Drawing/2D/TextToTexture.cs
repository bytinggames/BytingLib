using BytingLib.Markup;
using BytingLib.UI;

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
        /// <summary>Used for debugging</summary>
        public bool DrawTextFitPolygon { get; set; }
        /// <summary>Used for debugging</summary>
        public bool DrawTextFitSegments { get; set; }

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

                return CreateTextTexture(text, fontArray.GetFont(fontSize), backgroundColor, markupSettings.TextureScale, markupSettings.VerticalSpaceBetweenLines * fontSize);
            });
            return tex;
        }

        public Promise<Ref<Texture2D>> UseTexture(string text, Vector3 right, Color backgroundColor, List<List<Vector2>> polygons, Vector2 anchor, Vector2 texSize, float? verticalSpaceBetweenLines = null, Padding? paddingNormalized = null)
        {
            Promise<Ref<Texture2D>> tex = new(() =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return GetPixel();
                }
                var font = fontArray.GetFont(1f, out float actualFontSize);
                var markupSettings = new MarkupSettings(spriteBatch, font, Anchor.TopLeft(0, 0), Color.White)
                {
                    VerticalSpaceBetweenLines = verticalSpaceBetweenLines ?? this.verticalSpaceBetweenLines,
                    VerticalAlignInLine = anchor.Y,
                    HorizontalAlignInLine = anchor.X
                };
                var drawElement = new MarkupRoot(markupCreator, text);
                Vector2 textSize = drawElement.GetSize(markupSettings); // TODO: remove this?
                textSize /= actualFontSize;

                int fontSize = GetRightFontSize(right.Length() * 2f /* because right only measures half the length */,
                    (int)MathF.Ceiling(textSize.X), MinimumPixelsPerUnit);

                return CreateTextTexture(text, 
                    fontArray.GetFont(fontSize), 
                    backgroundColor,
                    polygons,
                    TextFillObject.PolyType.Normalized01, 
                    PolygonTextSplit.OnlyOnSpace, 
                    anchor, 
                    texSize,
                    markupSettings.TextureScale,
                    markupSettings.VerticalSpaceBetweenLines * fontSize,
                    paddingNormalized);
            });
            return tex;
        }

        private Ref<Texture2D> GetPixel()
        {
            return new AssetHolder<Texture2D>(spriteBatch.GetPixel(), "Pixel", _ => { }).Use();
        }

        private static int GetRightFontSize(float spaceInMeters, int textureWidthOfScale1, float targetPixelsPerCM)
        {
            float pixelsPerMeter = textureWidthOfScale1 / spaceInMeters;

            float requiredFontScale = targetPixelsPerCM / pixelsPerMeter;

            int fontSize = (int)MathF.Ceiling(requiredFontScale);
            fontSize = Math.Min(8, Math.Max(1, fontSize));
            return fontSize;
        }

        public Ref<Texture2D> CreateTextTexture(string text, Ref<SpriteFont> font, Color backgroundColor, Vector2? textureScale = null, float? verticalSpaceBetweenLines = null)
        {
            textureScale ??= Vector2.One;
            if (textures.ContainsKey((text, font.Value, backgroundColor, textureScale.Value)))
            {
                return textures[(text, font.Value, backgroundColor, textureScale.Value)].Use();
            }

            var markupSettings = new MarkupSettings(spriteBatch, font, Anchor.TopLeft(0, 0), Color.Black /* default text color is black */)
            {
                TextureScale = textureScale.Value,
                VerticalSpaceBetweenLines = verticalSpaceBetweenLines ?? this.verticalSpaceBetweenLines
            };
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

                disposables.Use(tex);
                return tex;
            });

            AssetHolder<Texture2D> assetHolder = new AssetHolder<Texture2D>(promise, "TextToTexture_" + text, _ =>
            {
                if (!textures.Remove((text, font.Value, backgroundColor, textureScale.Value)))
                {
                    throw new BytingException("couldn't remove a texture from TextToTexture.textures");
                }

                tex?.Dispose();
            });

            textures.Add((text, font.Value, backgroundColor, textureScale.Value), assetHolder);

            return assetHolder.Use();
        }

        public Ref<Texture2D> CreateTextTexture(string text, Ref<SpriteFont> font, Color backgroundColor, List<List<Vector2>> polygons, 
            TextFillObject.PolyType polyType, PolygonTextSplit splitMethod, Vector2 anchor, Vector2 texSize, Vector2? textureScale = null, float? verticalSpaceBetweenLines = null, Padding? paddingNormalized = null)
        {
            textureScale ??= Vector2.One;
            //if (textures.ContainsKey((text, font.Value, backgroundColor, textureScale)))
            //{
            //    return textures[(text, font.Value, backgroundColor, textureScale)].Use();
            //}

            var markupSettings = new MarkupSettings(spriteBatch, font, Anchor.TopLeft(0, 0), Color.Black /* default text color is black */)
            {
                VerticalSpaceBetweenLines = verticalSpaceBetweenLines ?? this.verticalSpaceBetweenLines,
                VerticalAlignInLine = anchor.Y,
                HorizontalAlignInLine = anchor.X,
                TextureScale = textureScale.Value
            };
            TextFillObject textFill = new(text, polygons, polyType, splitMethod, true, true);
            textFill.Anchor = anchor;
            textFill.PaddingNormalized = paddingNormalized;

            Rect rect = new Rect(Vector2.Zero, texSize);

            var drawElement = textFill.GetMarkup(font, markupCreator, rect)!;

            if (drawElement == null)
            {
                return GetPixel();
            }

            markupSettings.Scale = textFill.TextFill.FontScale;

            // scale so 1 input pixel (from textures and font) matches exactly 1 output pixel (from rendertarget)
            float drawScale = 1f / markupSettings.Scale.X; // for controlling the resoultion of the output image
            texSize *= drawScale;

            Rect texOutputRect = new Rect(0, 0, texSize.X, texSize.Y);
            if (paddingNormalized != null)
            {
                texOutputRect.ApplyNormalizedPadding(paddingNormalized);
                texOutputRect.RoundToLarger();
            }

            // previously:
            //texSize = drawElement.GetSize(markupSettings);

            RenderTarget2D? tex = null;

            Promise<Texture2D> promise = new(() =>
            {
                var gDevice = spriteBatch.GraphicsDevice;
                tex = new RenderTarget2D(gDevice, (int)Math.Ceiling(texOutputRect.Width), (int)Math.Ceiling(texOutputRect.Height), false, SurfaceFormat.Color, DepthFormat.None);

                using (gDevice.UseRenderTarget(tex))
                {
                    gDevice.Clear(Color.Transparent);

                    using (textEffect.Color.Use(backgroundColor.ToVector4()))
                    {
                        textEffect.ApplyParameters();
                        Matrix transform =
                            Matrix.CreateScale(drawScale)
                            * Matrix.CreateTranslation(new Vector3(-texOutputRect.Pos, 0f));
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, textEffect.Effect.Value, transform);

                        if (DrawTextFitPolygon)
                        {
                            textFill.TextFill?.DrawSegments(spriteBatch, Color.Lerp(Color.White, Color.Blue, 0.3f));
                        }
                        if (DrawTextFitSegments)
                        {
                            textFill.DrawPolygon(spriteBatch);
                        }
                        drawElement.Draw(markupSettings);

                        spriteBatch.End();
                    }
                }

                disposables.Use(tex);
                return tex;
            });

            AssetHolder<Texture2D> assetHolder = new AssetHolder<Texture2D>(promise, "TextToTexturePolygon_" + text, _ =>
            {
                //if (!textures.Remove((text, font.Value, backgroundColor, textureScale)))
                //{
                //    throw new BytingException("couldn't remove a texture from TextToTexture.textures");
                //}

                tex?.Dispose();
            });

            //textures.Add((text, font.Value, backgroundColor, textureScale), assetHolder);

            return assetHolder.Use();
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
