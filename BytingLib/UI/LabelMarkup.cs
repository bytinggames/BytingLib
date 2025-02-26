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

        private TextFillObject? textFill;
        public TextFillObject? TextFill
        {
            get => textFill;
            set
            {
                textFill = value;
                setSizeToText = false;
                Width = -1f;
                Height = -1f;
            }
        }

        public LabelMarkup(string text, Creator creator, float width = 0f, float height = 0f, bool setSizeToText = true, TextWrap wrap = TextWrap.AllowMidWordIfSpaceNotPossible)
            : base(text, width, height, setSizeToText)
        {
            this.creator = creator;

            if (width != 0f)
            {
                textFill = new TextFillObject(text, new(), TextFillObject.PolyType.Normalized01, wrap, true);
                if (width < 0f)
                {
                    this.setSizeToText = false;
                }
            }
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
            if (textFill != null)
            {
                //textFill.DrawPolygon(spriteBatch);
                //textFill.TextFill?.DrawSegments(spriteBatch, Color.Blue * 0.1f);

                if (!setSizeToText)
                {
                    UpdateMarkupWrapped(style);
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
                        TextureScale = style.MarkupTextureScale,
                        CropSuperfluousHeightThatIsLargerThanLineHeight = CropSuperfluousHeightThatIsLargerThanLineHeight,
                        JumpOffset = GetJumpOffset()
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
            return TextFill?.TextFill?.FontScale ?? style.FontScale;
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
                TextureScale = style.MarkupTextureScale,
                CropSuperfluousHeightThatIsLargerThanLineHeight = CropSuperfluousHeightThatIsLargerThanLineHeight,
                JumpOffset = GetJumpOffset(),
                VerticalAlignInLine = 0.5f,
            };
        }

        private Vector2 GetJumpOffset()
        {
            return (AbsoluteRect?.Pos ?? Vector2.Zero) + new Vector2(Padding?.Left ?? 0f, Padding?.Top ?? 0f);
        }

        protected override void DisposeSelf()
        {
            markup?.Dispose();
            markup = null;
        }

        protected override Label SetSizeToText(StyleRoot style)
        {
            if (textFill == null)
            {
                return base.SetSizeToText(style);
            }
            else
            {
                if (setSizeToText)
                {
                    textFill?.UpdatePolygons(new Rect(0, 0, initialWidth, 0f /* TODO: really 0?? or Height? or initialHeight? */));
                    UpdateMarkupWrapped(style);
                }
            }

            return this;
        }

        protected override void UpdateTreeBeginSelf(StyleRoot style)
        {
            textFill?.SetDirty(Text, Anchor); // trigger reloading

            base.UpdateTreeBeginSelf(style);

            if (TextFill == null)
            {
                UpdateMarkup();
            }
        }

        private void UpdateMarkupWrapped(StyleRoot style)
        {
            MarkupRoot? newMarkup = textFill?.GetMarkupIfUpdated(style.Font, creator);
            if (newMarkup != null)
            {
                markup?.Dispose();
                markup = newMarkup;

                if (setSizeToText)
                {
                    Vector2 size = newMarkup.GetSizeSubstring(GetDefaultSetting(null, style), new(newMarkup.Root)); //settings
                    Width = size.X;
                    Height = size.Y;
                }
            }
        }

        protected override void UpdateTreeInner(Rect rect)
        {
            base.UpdateTreeInner(rect);

            if (!setSizeToText)
            {
                textFill?.UpdatePolygons(rect);
            }
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

        public override void SetDirty()
        {
            textFill?.SetDirty(Text, Anchor); // trigger reloading

            base.SetDirty();
        }
    }
}
