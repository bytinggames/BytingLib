using BytingLib.Markup;

namespace BytingLib.UI
{
    public class LabelMarkup : Label
    {
        MarkupRoot? markup;
        private readonly Creator creator;

        public float MinLineHeight { get; set; }
        public double AnimationMillisecondsOffset { get; set; }
        /// <summary>see <see cref="MarkupSettings.CropSuperfluousHeightThatIsLargerThanLineHeight"/></summary>
        public bool CropSuperfluousHeightThatIsLargerThanLineHeight { get; set; } = false;

        private TextFillPolygon? fillPolygon;
        public TextFillPolygon? FillPolygon
        {
            get => fillPolygon;
            set
            {
                fillPolygon = value;
                setSizeToText = false;
                Width = -1f;
                Height = -1f;
            }
        }

        public LabelMarkup(string text, Creator creator) : base(text)
        {
            this.creator = creator;
        }
        public LabelMarkup(string text, Creator creator, float width = -1f, float height = -1f)
            : base(text, width, height)
        {
            this.creator = creator;
        }
        public LabelMarkup(string text, Creator creator, float width = -1f, float height = -1f, bool setSizeToText = true)
            : base(text, width, height, setSizeToText)
        {
            this.creator = creator;
        }

        protected override Vector2 MeasureString(StyleRoot style, string text)
        {
            using (MarkupRoot tempRoot = new MarkupRoot(creator, text))
            {
                return tempRoot.GetSize(GetDefaultSetting(null!, style));
            }
        }

        protected override string CreateTextToDraw(StyleRoot style, out List<(int Index, int Add)>? textLengthChanges)
        {
            textLengthChanges = null;
            return Text; // TODO: implement word wrapping?
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (FillPolygon != null)
            {
                FillPolygon.DrawPolygon(spriteBatch);
                FillPolygon.PolygonText?.DrawSegments(spriteBatch, Color.Blue * 0.1f);

                MarkupRoot? newMarkup = FillPolygon.GetMarkupIfUpdated(style, creator);
                if (newMarkup != null)
                {
                    markup?.Dispose();
                    markup = newMarkup;

                }
            }

            if (markup != null)
            {
                if (style.FontBoldColor.IsNotTransparent() && style.FontBold != null)
                {
                    markup.Draw(new MarkupSettings(spriteBatch, style.FontBold, AbsoluteRect.GetAnchor(Anchor), style.FontBoldColor, Anchor.X, GetFontScale(style), Tilt)
                    {
                        RoundPositionTo = style.RoundPositionTo,
                        MinLineHeight = MinLineHeight,
                        TotalMilliseconds = style.TotalMilliseconds - AnimationMillisecondsOffset,
                        ForceTextColor = true,
                        TextureColor = style.TextureColor ?? Color.White, // not sure if this should be the default for textures drawn with a bold font
                        CropSuperfluousHeightThatIsLargerThanLineHeight = CropSuperfluousHeightThatIsLargerThanLineHeight
                    });
                }

                if (style.FontColor.IsNotTransparent())
                {
                    markup.Draw(GetDefaultSetting(spriteBatch, style));
                }
            }
        }

        private Vector2 GetFontScale(StyleRoot style)
        {
            return FillPolygon?.PolygonText?.FontScale ?? style.FontScale;
        }

        private MarkupSettings GetDefaultSetting(SpriteBatch spriteBatch, StyleRoot style)
        {
            return new MarkupSettings(spriteBatch, 
                style.Font,
                AbsoluteRect == null ? new Anchor() : AbsoluteRect.GetAnchor(Anchor), 
                style.FontColor, 
                Anchor.X,
                GetFontScale(style),
                Tilt)
            { 
                RoundPositionTo = style.RoundPositionTo,
                MinLineHeight = MinLineHeight,
                TotalMilliseconds = style.TotalMilliseconds,
                TextureColor = style.TextureColor ?? Color.White,
                CropSuperfluousHeightThatIsLargerThanLineHeight = CropSuperfluousHeightThatIsLargerThanLineHeight
            };
        }

        protected override void DisposeSelf()
        {
            markup?.Dispose();
            markup = null;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            if (fillPolygon != null)
            {
                return; // fillPolygon updates in UpdateTreeInner()
            }

            base.UpdateTreeBeginSelf(style);

            if (FillPolygon == null)
            {
                UpdateMarkup();
            }
        }

        protected override void UpdateTreeInner(Rect rect)
        {
            base.UpdateTreeInner(rect);

            fillPolygon?.UpdateTreeInner(rect);
            fillPolygon?.SetDirty(Text, Anchor); // trigger reloading
        }

        private void UpdateMarkup()
        {
            markup?.Dispose();
            markup = new MarkupRoot(creator, TextToDraw); // TODO
        }

        public Vector2 MeasureSize(StyleRoot style)
        {
            if (style.FontBold != null && style.FontBoldColor.IsNotTransparent())
            {
                return style.FontBold.Value.MeasureString(Text) * style.FontScale;
            }
            return style.Font.Value.MeasureString(Text) * style.FontScale;
        }
    }
}
