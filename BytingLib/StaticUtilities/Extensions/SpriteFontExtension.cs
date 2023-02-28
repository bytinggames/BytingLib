using static BytingLib.Markup.MarkupSettings;

namespace BytingLib
{
    public static class SpriteFontExtension
    {
        //public class Line
        //{
        //    public Color Color = Color.Black;
        //    public float Thickness = 1f;
        //}

        //public class Outline : Line
        //{
        //    /// <summary>How often the text is drawn to generate the outline.</summary>
        //    public int Quality = 4;
        //}

        //public class Underline : Line
        //{
        //    public float Offset = 0f;
        //}

        //public static void Draw(this SpriteFont _font, SpriteBatch spriteBatch, string _text, Vector2 _position,
        //    Color? _color = null, Vector2? _scale = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None)
        //{
        //    Draw(_font, spriteBatch, _text, new Anchor(_position, Vector2.Zero), _color, _scale, _rotation, _effects);
        //}
        //public static void Draw(this SpriteFont _font, SpriteBatch spriteBatch, string _text, Anchor _anchor,
        //    Color? _color = null, Vector2? _scale = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None)
        //{
        //    _font.DrawFinal(spriteBatch, _text, _anchor, _color, _scale, _rotation, _effects);
        //}
        public static void Draw(this SpriteFont _font, SpriteBatch spriteBatch, string _text, Anchor _anchor,
            Color? _color = null, Vector2? _scale = null, float _rotation = 0f, SpriteEffects _effects = SpriteEffects.None,
            Underline? underline = null, Outline? outline = null, float roundPositionTo = 0f, int fat = 0)
        {
            Vector2 scale = _scale ?? Vector2.One;

            Vector2 textSize = Vector2.Zero;
            if (_anchor.origin != Vector2.Zero || underline != null)
                textSize = _font.MeasureString(_text);

            Vector2 origin = _anchor.origin * textSize;
            textSize *= scale;

            Vector2 pos = _anchor.pos;

            if (roundPositionTo != 0)
            {
                Vector2 shift = origin * scale;
                Vector2 drawPos = pos - shift;

                if (roundPositionTo == 1f)
                    drawPos = (drawPos - new Vector2(0.1f) /* to prevent 0.5 twitching */).GetRound();
                else
                    drawPos = (drawPos / roundPositionTo).GetRound() * roundPositionTo;

                pos = drawPos + shift;
            }

            if (underline != null)
            {
                Rect underlineRect = _anchor.Rectangle(textSize);
                underlineRect.Y += underlineRect.Height + underline.Thickness / 2f + underline.Offset;
                underlineRect.Y = MathF.Floor(underlineRect.Y);
                underlineRect.Height = underline.Thickness;
                underlineRect.Draw(spriteBatch, underline.Color, spriteBatch.DefaultDepth);
            }

            if (outline != null)
            {
                Color c = outline.Color;
                if (outline.Quality == 4)
                {
                    float depth = spriteBatch.DefaultDepth;
                    spriteBatch.DrawString(_font, _text, pos + new Vector2(outline.Thickness, 0), c, _rotation, origin, scale, _effects, depth);
                    spriteBatch.DrawString(_font, _text, pos + new Vector2(0, -outline.Thickness), c, _rotation, origin, scale, _effects, depth);
                    spriteBatch.DrawString(_font, _text, pos + new Vector2(-outline.Thickness, 0), c, _rotation, origin, scale, _effects, depth);
                    spriteBatch.DrawString(_font, _text, pos + new Vector2(0, outline.Thickness), c, _rotation, origin, scale, _effects, depth);
                }
                else
                {
                    float totalArc;
                    int count = outline.Quality;
                    if (count > 0)
                    {
                        totalArc = MathHelper.TwoPi;
                    }
                    else
                    {
                        totalArc = MathHelper.Pi;
                        count = -count;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        float angle = i * totalArc / count;
                        spriteBatch.DrawString(_font, _text, pos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * outline.Thickness, c, _rotation, origin, scale, _effects, spriteBatch.DefaultDepth);
                    }
                }
            }

            Color color = _color ?? Color.White;

            if (fat > 0)
            {
                float depth = spriteBatch.DefaultDepth;
                for (int i = 0; i < fat; i++)
                {
                    spriteBatch.DrawString(_font, _text, pos + new Vector2(i + 1, 0), color, _rotation, origin, scale, _effects, depth);
                }
            }

            spriteBatch.DrawString(_font, _text, pos, color, _rotation, origin, scale, _effects, spriteBatch.DefaultDepth);
        }
    }

    //public static class SpriteFontExtension
    //{
    //    private static Dictionary<SpriteFont, SpriteFontExtended> extendedData = new();

    //    public static string StringReplaceUnresolvableCharacters(this SpriteFont font, string text, string replaceWith)
    //    {
    //        for (int i = text.Length - 1; i >= 0; i--)
    //        {
    //            if (!font.Characters.Contains(text[i]))
    //                text = text.Remove(i, 1).Insert(i, replaceWith);
    //        }
    //        return text;
    //    }

    //    private static SpriteFontExtended GetExtended(SpriteFont font)
    //    {
    //        SpriteFontExtended? extended;
    //        if (extendedData.TryGetValue(spriteBatch, out extended))
    //            return extended;
    //        extended = new SpriteBatchExtended(spriteBatch);
    //        extendedData.Add(spriteBatch, extended);
    //        return extended;
    //    }
    //}


    //internal class SpriteFontExtended
    //{
    //    public SpriteBatchExtended(SpriteFont font)
    //    {
    //    }
    //}
}
