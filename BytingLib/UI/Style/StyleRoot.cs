namespace BytingLib.UI
{
    public class StyleRoot
    {
        public Ref<SpriteFont> Font => GetOverride(f => f.Font)!;
        public Ref<SpriteFont>? FontBold => GetOverride(f => f.FontBold);
        public Color? FontColor => GetOverride(f => f.FontColor);
        public Color? FontBoldColor => GetOverride(f => f.FontBoldColor);
        public Color? TextureColor => GetOverride(f => f.TextureColor);
        public Vector2 FontScale => GetOverrideMultiply(f => f.FontScale);
        public Ref<Animation> ButtonAnimation => GetOverride(f => f.ButtonAnimation)!;
        public Padding? ButtonPadding => GetOverride(f => f.ButtonPadding);
        public bool ButtonPaddingToButtonBorder => GetOverride(f => f.ButtonPaddingToButtonBorder)!.Value;
        public float RoundPositionTo => GetOverride(f => f.RoundPositionTo)!.Value;

        public StyleBase StyleBase { get; set; }
        private List<Style> styleOverrides = new();
        public double TotalMilliseconds { get; set; }

        public float LineSpacing => Font.Value.LineSpacing * FontScale.Y;

        public Matrix SpriteBatchTransform { get; internal set; }
        private readonly Stack<Effect> customEffects = new();
        private readonly Stack<SpriteSortMode> customSortModes = new();

        /// <summary>Store the SpriteBatch.Begin() call here, so we can flush all spritebatch calls and make new ones that get cut with a rasterizer scissor rectangle.</summary>
        internal BatchBeginAction? SpriteBatchBegin { get; set; }

        internal delegate void BatchBeginAction(bool scissorTest, Effect? customEffect, SpriteSortMode? sortMode);

        public StyleRoot(StyleBase baseStyle)
        {
            StyleBase = baseStyle;
            styleOverrides.Add(baseStyle);
        }

        private T? GetOverride<T>(Func<Style, T?> get)
        {
            for (int i = styleOverrides.Count - 1; i >= 0; i--)
            {
                T? value = get(styleOverrides[i]);
                if (value != null)
                {
                    return value;
                }
            }
            return default;
        }

        private Vector2 GetOverrideMultiply(Func<Style, Vector2?> get)
        {
            Vector2 product = Vector2.One;
            for (int i = styleOverrides.Count - 1; i >= 0; i--)
            {
                Vector2? value = get(styleOverrides[i]);
                if (value != null)
                {
                    product *= value.Value;
                }
            }
            return product;
        }

        public void Push(Style? styleOverride)
        {
            if (styleOverride == null)
            {
                return;
            }

            styleOverrides.Add(styleOverride);
        }

        public void Pop(Style? styleOverride)
        {
            if (styleOverride == null)
            {
                return;
            }

            styleOverrides.RemoveAt(styleOverrides.Count - 1);
        }

        public Vector2 MeasureString(string text)
        {
            return Font.Value.MeasureString(text) * FontScale;
        }

        public void ScissorRect(SpriteBatch spriteBatch, Rect rect, Action draw)
        {
            if (SpriteBatchBegin == null)
            {
                throw new Exception("before using Scissor() you need to set the Begin action of StyleRoot");
            }

            Rect absoluteRectWindowSpace = rect.GetTransformed(SpriteBatchTransform);

            spriteBatch.End();
            //spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            SpriteBatchBegin(true, null, null);
            bool rememberScissorTest = spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable;
            using (CodeHelper.ChangeVarTemporarily(spriteBatch.GraphicsDevice.ScissorRectangle,
                f => spriteBatch.GraphicsDevice.ScissorRectangle = f,
                absoluteRectWindowSpace.ToRectangle()
                ))
            {
                draw();
                spriteBatch.End();
            }
            //spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = rememberScissorTest;
            SpriteBatchBegin(rememberScissorTest, null, null);
        }

        public void UseEffect(SpriteBatch spriteBatch, Effect effect, SpriteSortMode? sortMode, Action draw)
        {
            if (SpriteBatchBegin == null)
            {
                throw new Exception("before using UseEffect() you need to set the Begin action of StyleRoot");
            }

            spriteBatch.End();
            customEffects.Push(effect);
            if (sortMode != null)
            {
                customSortModes.Push(sortMode.Value);
            }
            SpriteBatchBegin(spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable, effect, sortMode);
            
            draw();

            spriteBatch.End();
            customEffects.Pop();
            if (sortMode != null)
            {
                customSortModes.Pop();
            }
            SpriteBatchBegin(spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable,
                customEffects.Count == 0 ? null : customEffects.Peek(),
                customSortModes.Count == 0 ? null : customSortModes.Peek()
                );
        }
    }
}
