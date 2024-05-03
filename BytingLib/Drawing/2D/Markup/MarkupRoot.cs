namespace BytingLib.Markup
{
    public class MarkupRoot : IDisposable
    {
        MarkupCollection root;

        Action? unsubscribeOnDispose;

        /// <summary>Don't forget to dispose!</summary>
        public MarkupRoot(Creator creator, string text)
        {
            root = new MarkupCollection(creator, text);
        }

        /// <summary>Don't forget to dispose!</summary>
        public MarkupRoot(Creator creator, Func<string> text, ILocaChanger loca)
        {
            root = new MarkupCollection(creator, text());
            Action a = () => root = new MarkupCollection(creator, text());
            loca.OnLocaReload += a;
            unsubscribeOnDispose += () => loca.OnLocaReload -= a;
        }

        public void Draw(MarkupSettings _settings)
        {
            (Vector2 totalSize, Vector2[] lineSizes, float marginTop, float marginBottom) = GetSizes(_settings);
            Vector2 topLeft = _settings.Anchor.Rectangle(totalSize.X, totalSize.Y + marginTop + marginBottom).TopLeft;
            Vector2 topLeftOfLine = topLeft;
            topLeftOfLine.Y += marginTop;
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
            var linesSizes = GetLinesSizes(settings, out float marginTop, out float marginBottom);
            foreach (var size in linesSizes)
            {
                totalSize.Y += size.Y;
                if (size.X > totalSize.X)
                {
                    totalSize.X = size.X;
                }
            }
            totalSize.Y += marginTop + marginBottom;
            return totalSize;
        }

        public (Vector2 totalSize, Vector2[] lineSizes, float marginTop, float marginBottom) GetSizes(MarkupSettings settings)
        {
            Vector2 totalSize = new Vector2();
            Vector2[] lineSizes = GetLinesSizes(settings, out float marginTop, out float marginBottom).ToArray();
            foreach (var size in lineSizes)
            {
                totalSize.Y += size.Y;
                if (size.X > totalSize.X)
                {
                    totalSize.X = size.X;
                }
            }
            return (totalSize, lineSizes, marginTop, marginBottom);
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

        public List<Vector2> GetLinesSizes(MarkupSettings settings, out float marginTop, out float marginBottom)
        {
            marginTop = marginBottom = 0;

            bool firstLine = true;

            List<Vector2> sizes = new();

            int lineCount = GetLineCount();
            int lineIndex = 0;

            foreach (var line in GetLinesOfLeaves(settings))
            {
                bool lastLine = lineIndex == lineCount - 1;

                sizes.Add(GetLineSize(settings, firstLine, line, out float? cropped));

                if (cropped != null)
                {
                    if (firstLine)
                    {
                        marginTop = cropped.Value * settings.VerticalAlignInLine;
                    }
                    if (lastLine)
                    {
                        marginBottom = cropped.Value * (1f - settings.VerticalAlignInLine);
                    }
                }
                firstLine = false;
                lineIndex++;
            }

            return sizes;
        }

        private static Vector2 GetLineSize(MarkupSettings settings, bool firstLine, IEnumerable<ILeaf> line, out float? croppedBecauseOfLineHeight)
        {
            croppedBecauseOfLineHeight = null;

            Vector2 lineSize = new Vector2(0, settings.MinLineHeight);
            bool allElementsConfineToLineSpacing = true;
            foreach (var element in line)
            {
                Vector2 size = element.GetSize(settings);
                lineSize.X += size.X;
                if (size.Y > lineSize.Y)
                {
                    lineSize.Y = size.Y;
                }

                if (!element.ConfinesToLineSpacing)
                {
                    allElementsConfineToLineSpacing = false;
                }
            }

            // crop to line spacing, when:
            if (allElementsConfineToLineSpacing) // all elements in the line support LineSpacing
            {
                if (lineSize.Y > settings.Font.Value.LineSpacing)
                {
                    croppedBecauseOfLineHeight = lineSize.Y - settings.Font.Value.LineSpacing;
                    lineSize.Y = settings.Font.Value.LineSpacing;
                }
            }

            if (!firstLine)
            {
                lineSize.Y += settings.VerticalSpaceBetweenLines;
            }

            return lineSize;
        }

        public IEnumerable<IEnumerable<ILeaf>> GetLinesOfLeaves(MarkupSettings settings)
        {
            IEnumerator<ILeaf> enumerator = root.IterateOverLeaves(settings).GetEnumerator();
            
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

        public int GetLineCount()
        {
            int newLineCount = root.Children.OfType<MarkupNewLine>().Count();
            return newLineCount + 1;
        }

        public override string ToString()
        {
            return string.Join(" ", root.Children.Select(f => f.ToString()));
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
            return GetRectangle(markupSettings, (markupSettings.Font.Value.LineSpacing - markupSettings.Font.Value.DefaultCharacterHeight) * markupSettings.Scale.Y);
        }

        public void Dispose()
        {
            unsubscribeOnDispose?.Invoke();
            unsubscribeOnDispose = null;

            root.Dispose();
        }
    }
}
