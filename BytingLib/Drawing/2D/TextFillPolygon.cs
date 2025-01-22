using BytingLib.Markup;
using BytingLib.UI;

namespace BytingLib
{
    public class TextFillPolygon
    {
        private readonly List<List<Vector2>> polygons;
        private readonly PolyType polyType;
        private readonly bool borderLeft;
        private readonly bool borderRight;
        private List<List<Vector2>>? polygonsTransformed;
        public PolygonText? PolygonText { get; private set; }
        private PolygonTextSplit splitMethod;
        private Rect? AbsoluteRect;

        public bool GlobalAnchor { get; set; } = true;
        public Vector2 Anchor { get; set; } // TODO: update
        public string Text { get; set; } // TODO: update
        public string? MarkupTextOutput { get; private set; }

        public TextFillPolygon(string text, List<List<Vector2>> polygons, PolyType polyType, PolygonTextSplit splitMethod, bool borderLeft = true, bool borderRight = true)
        {
            this.Text = text;
            this.polygons = polygons;
            this.polyType = polyType;
            this.splitMethod = splitMethod;
            this.borderLeft = borderLeft;
            this.borderRight = borderRight;
            if (polyType == PolyType.Absolute)
            {
                polygonsTransformed = polygons;
            }
        }


        public enum PolyType
        {
            /// <summary>[0,0] -> AbsoluteRect.TopLeft [1,1] -> AbsoluteRect.BottomRight</summary>
            Normalized01,
            /// <summary>Polygons stay exactly as provided</summary>
            Absolute,
            /// <summary>Polygons are shifted by AbsoluteRect.TopLeft</summary>
            Relative
        }

        internal void UpdateTreeInner(Rect rect)
        {
            AbsoluteRect = rect;
            PolygonText = null; // trigger reloading

            switch (polyType)
            {
                case PolyType.Normalized01:
                    polygonsTransformed = polygons.Select(f => f.ToList()).ToList();
                    foreach (var polygon in polygonsTransformed)
                    {
                        for (int i = 0; i < polygon.Count; i++)
                        {
                            polygon[i] *= rect.Size;
                            polygon[i] += rect.TopLeft;
                        }
                    }
                    break;
                case PolyType.Absolute:
                    // nothing to do
                    break;
                case PolyType.Relative:
                    polygonsTransformed = polygons.Select(f => f.ToList()).ToList();
                    foreach (var polygon in polygonsTransformed)
                    {
                        for (int i = 0; i < polygon.Count; i++)
                        {
                            polygon[i] += rect.TopLeft;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
        public void DrawPolygon(SpriteBatch spriteBatch)
        {
            if (polygonsTransformed != null)
            {
                foreach (var polygon in polygonsTransformed)
                {
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        int j = (i + 1) % polygon.Count;
                        spriteBatch.DrawLine(polygon[i], polygon[j], Color.Green, 1f);
                    }
                }
            }
        }

        public void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (polygonsTransformed != null)
            {
                DrawPolygon(spriteBatch);

                if (PolygonText == null)
                {
                    UpdateText(style.Font, null);
                }

                PolygonText?.DrawSegments(spriteBatch, Color.Blue * 0.1f);
            }
        }

        private void UpdateText(Ref<SpriteFont> font, Creator? creator)
        {
            if (polygonsTransformed != null && AbsoluteRect != null)
            {
                PolygonText = new PolygonText(Text, font, AbsoluteRect, Anchor, GlobalAnchor, polygonsTransformed, splitMethod, borderLeft, borderRight, creator);
            }
        }

        internal MarkupRoot? UpdateMarkup(StyleRoot style, Creator creator)
        {
            if (PolygonText == null)
            {
                UpdateText(style.Font, creator);
                if (PolygonText != null)
                {
                    return PolygonText.SegmentedMarkup;
                    //return PolygonText.GetSegmentedMarkupText();
                    //throw new NotImplementedException();
                    //return string.Join('\n', flexText.segmentedText);// "test #move(10|10)#c(f00|red)";
                }
            }
            return null;
        }

        public void SetDirty(string text, Vector2 anchor)
        {
            PolygonText = null;
            Text = text;
            Anchor = anchor;
        }
    }
}