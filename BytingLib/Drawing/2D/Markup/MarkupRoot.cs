using System.Diagnostics.CodeAnalysis;

namespace BytingLib.Markup
{
    public class MarkupRoot : IDisposable
    {
        public MarkupCollection Root { get; private set; }

        Action? unsubscribeOnDispose;

        /// <summary>Don't forget to dispose!</summary>
        public MarkupRoot(Creator creator, string text)
        {
            Root = new MarkupCollection(creator, text);
        }

        /// <summary>Don't forget to dispose!</summary>
        public MarkupRoot(Creator creator, Func<string> text, ILocaChanger loca)
        {
            Root = new MarkupCollection(creator, text());
            Action a = () => Root = new MarkupCollection(creator, text());
            loca.OnLocaReload += a;
            unsubscribeOnDispose += () => loca.OnLocaReload -= a;
        }

        public MarkupRoot(MarkupCollection root)
        {
            Root = root;
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
                settings.Anchor = new Anchor(lineBounds.X, lineBounds.Y + settings.VerticalAlignInLine * lineSize.Y, 0f, settings.VerticalAlignInLine);
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

        public Vector2 GetSizeSubstring(MarkupSettings settings, MarkupIndex start, MarkupIndex? end = null)
        {
            return GetSizeSubstring(settings, start, end, out _, out _);
        }

        public Vector2 GetSizeSubstring(MarkupSettings settings, MarkupIndex start, MarkupIndex? end, out float marginTop, out float marginBottom)
        {
            marginTop = marginBottom = 0;

            bool firstLine = true;

            int lineCount = GetLineCount();
            int lineIndex = 0;

            Vector2 totalSize = Vector2.Zero;

            int startOrEndFound = 0; // 1: start found 2: end found

            foreach (var line in GetLinesOfLeaves(settings))
            {
                bool lastLine = lineIndex == lineCount - 1;

                Vector2? size = GetLineSize(settings, firstLine, line, out float? cropped, start, end, ref startOrEndFound);
                if (size != null)
                {
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

                    totalSize.X = MathF.Max(size.Value.X, totalSize.X);
                    totalSize.Y += size.Value.Y;
                }

                if (startOrEndFound >= 2)
                {
                    break;
                }
            }
            return totalSize;
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
                    if (!settings.CropSuperfluousHeightThatIsLargerThanLineHeight)
                    {
                        croppedBecauseOfLineHeight = lineSize.Y - settings.Font.Value.LineSpacing;
                    }
                    lineSize.Y = settings.Font.Value.LineSpacing;
                }
            }

            if (!firstLine)
            {
                lineSize.Y += settings.VerticalSpaceBetweenLines;
            }

            return lineSize;
        }

        private static Vector2? GetLineSize(MarkupSettings settings, bool firstLine, IEnumerable<ILeaf> line, out float? croppedBecauseOfLineHeight,
            MarkupIndex start, MarkupIndex? end, ref int startOrEndFound)
        {
            croppedBecauseOfLineHeight = null;

            Vector2 lineSize = new Vector2(0, settings.MinLineHeight);
            bool allElementsConfineToLineSpacing = true;
            foreach (var element in line)
            {
                if (startOrEndFound == 0)
                {
                    if (element == start.CurrentNode)
                    {
                        startOrEndFound = 1;
                    }
                }
                

                if (startOrEndFound == 1)
                {
                    Vector2 size;

                    if (element == end?.CurrentNode && end.indexInString == 0)
                    {
                        startOrEndFound = 2;
                        break;
                    }

                    size = element.GetSize(settings,
                        start.CurrentNode == element ? start.indexInString : 0,
                        end != null && end.CurrentNode == element ? end.indexInString : -1);

                    lineSize.X += size.X;
                    if (size.Y > lineSize.Y)
                    {
                        lineSize.Y = size.Y;
                    }

                    if (!element.ConfinesToLineSpacing)
                    {
                        allElementsConfineToLineSpacing = false;
                    }

                    if (element == end?.CurrentNode)
                    {
                        startOrEndFound = 2;
                        break;
                    }
                }
            }

            if (startOrEndFound == 0)
            {
                return null;
            }
            // crop to line spacing, when:
            if (allElementsConfineToLineSpacing) // all elements in the line support LineSpacing
            {
                if (lineSize.Y > settings.Font.Value.LineSpacing)
                {
                    if (!settings.CropSuperfluousHeightThatIsLargerThanLineHeight)
                    {
                        croppedBecauseOfLineHeight = lineSize.Y - settings.Font.Value.LineSpacing;
                    }
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

        public int GetLineCount()
        {
            int newLineCount = Root.AllChildren().OfType<MarkupNewLine>().Count();
            return newLineCount + 1;
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
            return GetRectangle(markupSettings, (markupSettings.Font.Value.LineSpacing - markupSettings.Font.Value.DefaultCharacterHeight) * markupSettings.Scale.Y);
        }

        public void Dispose()
        {
            unsubscribeOnDispose?.Invoke();
            unsubscribeOnDispose = null;

            Root.Dispose();
        }


        // todo: navigate to MarkupIndex inside markup
        // todo: measure from index to index

        public char? this[MarkupIndex index]
        {
            get
            {
                if (index.CurrentNode is MarkupText markupText)
                {
                    return markupText.Text[index.indexInString];
                }
                else
                {
                    return null;
                }
                //int level = 0;
                //INode currentNode = Root;
                //while (level + 1 < index.indexTree.Count)
                //{
                //    if (currentNode is MarkupCollection collection)
                //    {
                //        currentNode = collection.Children[index[level]];
                //        level++;
                //    }
                //}
                //return currentNode.GetChar(index[level]);
            }
        }

        public void InsertJump(MarkupIndex index, Vector2 jump, params MarkupIndex[] indicesToMaybeCorrect)
        {
            if (index.CurrentNode is MarkupText text)
            {
                // replace current text node with markup collection that holds text + jump + text

                MarkupCollection parent = (index.selectedNodeHierarchy[^2] as MarkupCollection)!;

                int childIndex = parent.Children.IndexOf(text);

                MarkupText newText = new MarkupText(text.Text.Substring(index.indexInString));
                parent.Children.Insert(childIndex + 1, new MarkupJump(jump));
                parent.Children.Insert(childIndex + 2, newText);

                // split text into two
                text.Text = text.Text.Substring(0, index.indexInString);
                if (text.Text.Length == 0)
                {
                    // if first half has no length, simply remove that text part
                    parent.Children.RemoveAt(childIndex);
                }

                // goto new text node
                index.selectedNodeHierarchy[^1] = newText;
                index.indexInString = 0;


                for (int i = 0; i < indicesToMaybeCorrect.Length; i++)
                {
                    if (indicesToMaybeCorrect[i].CurrentNode == text)
                    {
                        if (indicesToMaybeCorrect[i].indexInString >= text.Text.Length)
                        {
                            indicesToMaybeCorrect[i].indexInString -= text.Text.Length;
                            indicesToMaybeCorrect[i].selectedNodeHierarchy[^1] = newText;
                        }
                    }
                }
            }
            else
            {
                // insert jump before current index
                if (!index.AtEnd())
                {
                    MarkupCollection parent = (index.selectedNodeHierarchy[^2] as MarkupCollection)!;
                    int childIndex = parent.Children.IndexOf(index.CurrentNode);
                    parent.Children.Insert(childIndex, new MarkupJump(jump));
                }
            }
        }
    }

    public class MarkupIndex
    {
        public List<INode> selectedNodeHierarchy;
        public int indexInString;
        public INode? CurrentNode => selectedNodeHierarchy.Count == 0 ? null :  selectedNodeHierarchy[^1];
        public MarkupIndex(INode root)
        {
            selectedNodeHierarchy = new() { root };

            // if the current node is a branch, go down until you hit a leaf
            while (CurrentNode is MarkupCollection collection)
            {
                selectedNodeHierarchy.Add(collection.Children[0]);
            }
        }

        public MarkupIndex Clone()
        {
            var clone = (MarkupIndex)MemberwiseClone();
            clone.selectedNodeHierarchy = selectedNodeHierarchy.ToList();
            return clone;
        }


        //public int this[int level]
        //{
        //    get => indexTree[level];
        //    set => indexTree[level] = value;
        //}

        public static MarkupIndex operator ++(MarkupIndex a)
        {
            if (a.AtEnd())
            {
                return a;
            }
            if (a.CurrentNode is MarkupText textNode && a.indexInString < textNode.Text.Length - 1)
            {
                a.indexInString++;
            }
            else
            {
                a.indexInString = 0; // start from the beginning of the next string
                while (a.selectedNodeHierarchy.Count >= 2)
                {
                    var parent = a.selectedNodeHierarchy[^2] as MarkupCollection;
                    int indexOfCurrentChild = parent.Children.IndexOf(a.CurrentNode);
                    indexOfCurrentChild++;
                    if (indexOfCurrentChild < parent.Children.Count)
                    {
                        a.selectedNodeHierarchy[^1] = parent.Children[indexOfCurrentChild];
                        break;
                    }
                    else
                    {
                        // remove last node, go up one level
                        a.selectedNodeHierarchy.RemoveAt(a.selectedNodeHierarchy.Count - 1);
                    }
                }

                if (a.selectedNodeHierarchy.Count == 1)
                {
                    // end reached
                    a.selectedNodeHierarchy.Clear();
                }
                else
                {
                    // if the current node is a branch, go down until you hit a leaf
                    while (a.CurrentNode is MarkupCollection collection)
                    {
                        a.selectedNodeHierarchy.Add(collection.Children[0]);
                    }
                }
            }
            return a;
        }

        public static MarkupIndex operator +(MarkupIndex a, int val)
        {
            a = a.Clone();
            while (val > 0)
            {
                a++;
                val--;
            }
            return a;
        }

        public static MarkupIndex operator --(MarkupIndex a)
        {
            if (a.indexInString > 0)
            {
                a.indexInString--;
            }
            else
            {
                while (a.selectedNodeHierarchy.Count >= 2)
                {
                    var parent = a.selectedNodeHierarchy[^2] as MarkupCollection;
                    int indexOfCurrentChild = parent.Children.IndexOf(a.CurrentNode);
                    indexOfCurrentChild--;
                    if (indexOfCurrentChild >= 0)
                    {
                        a.selectedNodeHierarchy[^1] = parent.Children[indexOfCurrentChild];
                        break;
                    }
                    else
                    {
                        // remove last node, go up one level
                        a.selectedNodeHierarchy.RemoveAt(a.selectedNodeHierarchy.Count - 1);
                    }
                }

                // if the current node is a branch, go down until you hit a leaf
                while (a.CurrentNode is MarkupCollection collection)
                {
                    a.selectedNodeHierarchy.Add(collection.Children[^1]);
                }

                if (a.CurrentNode is MarkupText markupText)
                {
                    a.indexInString = markupText.Text.Length - 1; // start at the end of the next string
                }
                else
                {
                    a.indexInString = 0;
                }
            }
            return a;
        }

        public static MarkupIndex operator -(MarkupIndex a, int val)
        {
            a = a.Clone();
            while (val > 0)
            {
                a--;
                val--;
            }
            return a;
        }

        public bool AtStart()
        {
            if (indexInString > 0)
            {
                return false;
            }

            for (int i = 0; i < selectedNodeHierarchy.Count - 1; i++)
            {
                if (selectedNodeHierarchy[i] is MarkupCollection collection)
                {
                    if (collection.Children.IndexOf(selectedNodeHierarchy[i + 1]) > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [MemberNotNullWhen(false, nameof(CurrentNode))]
        public bool AtEnd()
        {
            return selectedNodeHierarchy.Count == 0;
            //return index.selectedNodeHierarchy.Count == 0;
            //int level = 0;
            //INode currentNode = Root;
            //while (level + 1 < index.indexTree.Count)
            //{
            //    if (currentNode is MarkupCollection collection)
            //    {
            //        currentNode = collection.Children[index[level]];
            //        level++;
            //    }
            //}
            //return currentNode.GetChar(index[level]);
        }

        public bool IsEqual(MarkupIndex? index)
        {
            if (index == null)
            {
                return false;
            }
            return indexInString == index.indexInString
                && selectedNodeHierarchy.SequenceEqual(index.selectedNodeHierarchy);
        }

        public override string ToString()
        {
            if (CurrentNode is MarkupText text)
            {
                return text.Text.Insert(indexInString, "|");
            }
            return CurrentNode?.GetType().ToString() ?? "no node";
        }
    }

}
