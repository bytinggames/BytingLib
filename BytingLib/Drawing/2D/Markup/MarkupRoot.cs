using BytingLib.Creation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BytingLib.Markup
{
    public class MarkupRoot : IDisposable
    {
        MarkupCollection Root { get; }

        public MarkupRoot(Creator creator, string text)
        {
            Root = new MarkupCollection(creator, text);
        }

        public void Draw(MarkupSettings _settings)
        {
            (Vector2 totalSize, Vector2[] lineSizes) = GetSizes(_settings);
            Vector2 topLeft = _settings.Anchor.Rectangle(totalSize.X, totalSize.Y).TopLeft;
            Vector2 topLeftOfLine = topLeft;
            MarkupSettings settings = _settings.CloneMarkupSettings(); // clone to modify the anchor

            int lineIndex = 0;

            foreach (var line in GetLinesOfLeaves(settings))
            {
                Vector2 lineSize = lineSizes[lineIndex];

                if (settings.VerticalSpaceBetweenLines != 0 && lineIndex > 0 /* first line doesn't have that top space */)
                {
                    lineSize.Y -= settings.VerticalSpaceBetweenLines;
                    topLeftOfLine.Y += settings.VerticalSpaceBetweenLines;
                }

                float emptyHorizontalSpace = totalSize.X - lineSize.X;
                Rect lineBounds = new Rect(topLeftOfLine.X + settings.HorizontalAlignInLine * emptyHorizontalSpace, topLeftOfLine.Y, lineSize.X, lineSize.Y);
                settings.Anchor = new Anchor(lineBounds.X, lineBounds.Y + settings.VerticalAlignInLine * lineSize.Y, 0, settings.VerticalAlignInLine);
                foreach (var element in line)
                {
                    element.Draw(settings);
                    settings.Anchor.X += element.GetSize(settings).X;
                }
                topLeftOfLine.X = topLeft.X;
                topLeftOfLine.Y += lineSize.Y;

                lineIndex++;
            }
        }

        public Vector2 GetSize(MarkupSettings settings)
        {
            Vector2 totalSize = new Vector2();
            foreach (var size in GetLinesSizes(settings))
            {
                totalSize.Y += size.Y;
                if (size.X > totalSize.X)
                    totalSize.X = size.X;
            }
            return totalSize;
        }

        public (Vector2 totalSize, Vector2[] lineSizes) GetSizes(MarkupSettings settings)
        {
            Vector2 totalSize = new Vector2();
            Vector2[] lineSizes = GetLinesSizes(settings).ToArray();
            foreach (var size in lineSizes)
            {
                totalSize.Y += size.Y;
                if (size.X > totalSize.X)
                    totalSize.X = size.X;
            }
            return (totalSize, lineSizes);
        }

        public class LineWithSize
        {
            public Vector2 size;
            public IEnumerable<INode> elements;

            public LineWithSize(Vector2 size, IEnumerable<INode> elements)
            {
                this.size = size;
                this.elements = elements;
            }
        }

        public IEnumerable<Vector2> GetLinesSizes(MarkupSettings settings)
        {
            bool firstLine = true;
            foreach (var line in GetLinesOfLeaves(settings))
            {
                Vector2 lineSize = new Vector2(0, settings.MinLineHeight);
                foreach (var element in line)
                {
                    Vector2 size = element.GetSize(settings);
                    lineSize.X += size.X;
                    if (size.Y > lineSize.Y)
                        lineSize.Y = size.Y;
                }

                if (firstLine)
                    firstLine = false;
                else
                    lineSize.Y += settings.VerticalSpaceBetweenLines;

                yield return lineSize;
            }
        }

        public IEnumerable<IEnumerable<ILeaf>> GetLinesOfLeaves(MarkupSettings settings)
        {
            IEnumerator<ILeaf> enumerator = Root.IterateOverLeaves(settings).GetEnumerator();
            
            while (enumerator.MoveNext())
            {
                yield return GetNextLineOfLeaves(enumerator);
            }
        }

        public IEnumerable<ILeaf> GetNextLineOfLeaves(IEnumerator<ILeaf> enumerator)
        {
            do
            {
                if (enumerator.Current is MarkupNewLine)
                {
                    yield return enumerator.Current;
                    yield break;
                }
                yield return enumerator.Current;
            }
            while (enumerator.MoveNext());
        }

        public override string ToString()
        {
            return string.Join(" ", Root.Children.Select(f => f.ToString()));
        }

        public Rect GetRectangle(MarkupSettings markupSettings)
        {
            return markupSettings.Anchor.Rectangle(GetSize(markupSettings));
        }
        public Rect GetRectangle(MarkupSettings markupSettings, float enlarge)
        {
            var rect = markupSettings.Anchor.Rectangle(GetSize(markupSettings));
            return rect.Grow(enlarge);
        }
        public Rect GetRectangle(MarkupSettings markupSettings, float enlargeX, float enlargeY)
        {
            var rect = markupSettings.Anchor.Rectangle(GetSize(markupSettings));
            return rect.Grow(enlargeX, enlargeY);
        }
        public Rect GetRectangleEnlargeRelative(MarkupSettings markupSettings, float enlargeRelative)
        {
            var rect = markupSettings.Anchor.Rectangle(GetSize(markupSettings));
            return rect.Grow(rect.Size * enlargeRelative);
        }
        public Rect GetRectangleFontBased(MarkupSettings markupSettings)
        {
            return GetRectangle(markupSettings, markupSettings.Font.Value.LineSpacing - markupSettings.Font.Value.DefaultCharacterHeight);
        }

        public void Dispose()
        {
            Root.Dispose();
        }
    }
}
