using BytingLib.Markup;
using BytingLib.UI;

namespace BytingLib
{
    public class PolygonText
    {
        public Vector2 FontScale { get; private set; }
        private readonly List<string> segmentedText;
        public MarkupRoot? SegmentedMarkup { get; } = null;
        private readonly List<SegmentedLine> segmentedLines;
        private readonly float textTop;
        private readonly Vector2 anchor;
        private readonly bool globalAnchor;
        //private readonly float lineSpacing;
        List<Rect> segments = new();

        public PolygonText(string text, Ref<SpriteFont> font, Rect containerRect, Vector2 anchor, bool globalAnchor, List<List<Vector2>> polygons, PolygonTextSplit splitMethod,
            bool onlyAllowTextWhenAllPolygonsOverlaps = false, bool borderLeft = true, bool borderRight = true, Creator? creator = null)
        {
            this.anchor = anchor;
            this.globalAnchor = globalAnchor;
            FontScale = Vector2.One;
            textTop = 0f;
            segmentedLines = new();
            segmentedText = new();

            float overflow = float.PositiveInfinity;
            float totalSegmentsWidth = 0f;
            Vector2? minFontScale = null;
            Vector2? maxFontScale = null;

            bool incrementedLines = false;

            float defaultLineHeight = font.Value.LineSpacing * FontScale.Y;
            //int lines = (int)MathF.Floor(containerRect.Height / lineHeights);

            for (int iteration = 0; iteration < 1/*0*/ || overflow > 0f; iteration++)
            {
                if (incrementedLines)
                {
                    break;
                }

                if (overflow != float.PositiveInfinity)
                {
                    if (iteration == 0 && overflow > 0f)
                    {
                        iteration--; // retry
                    }

                    //if (iteration < 0)
                    {
                        // try font scaling
                        if (overflow < 0f)
                        {
                            minFontScale = FontScale;
                        }
                        else
                        {
                            maxFontScale = FontScale;
                        }

                        float textUsage = totalSegmentsWidth + overflow;
                        float usage = textUsage / totalSegmentsWidth;
                        float scaleFontBy = 1f / usage;
                        scaleFontBy = MathF.Sqrt(scaleFontBy); // because font is scaled in x and y direction
                        scaleFontBy *= 0.99f; // go a bit smaller, underflowing is better than overflowing, as we can take that underflowed output
                        FontScale *= scaleFontBy;

                        if (minFontScale != null && FontScale.X < minFontScale.Value.X)
                        {
                            if (maxFontScale == null)
                            {
                                FontScale = minFontScale.Value * 1.5f;
                            }
                            else
                            {
                                FontScale = (maxFontScale.Value + minFontScale.Value) / 2f;
                            }
                        }
                        else if (maxFontScale != null && FontScale.X > maxFontScale.Value.X)
                        {
                            if (minFontScale == null)
                            {
                                FontScale = maxFontScale.Value / 1.5f;
                            }
                            else
                            {
                                FontScale = (maxFontScale.Value + minFontScale.Value) / 2f;
                            }
                        }

                        defaultLineHeight = font.Value.LineSpacing * FontScale.Y;
                        //lines = (int)MathF.Floor(containerRect.Height / lineHeights);
                    }
                    //else
                    //{
                    //    if (overflow < 0)
                    //    {
                    //        // try reducing lines
                    //        lines--;
                    //    }
                    //    else
                    //    {
                    //        lines++;
                    //        incrementedLines = true;
                    //    }
                    //}
                }

                segmentedLines.Clear();
                segmentedText.Clear();

                Vector2 anchorPos = containerRect.GetPos(anchor);


                //spriteBatch.DrawLine(new Vector2(minX, anchorPos.Y), new Vector2(maxX, anchorPos.Y), Color.Red);

                float totalTextHeight = containerRect.Height; /* for now simply use entire height *///lineSpacing * lines;
                textTop = anchorPos.Y - totalTextHeight * anchor.Y;
                float textBottom = textTop + totalTextHeight;
                float cursorTop = textTop;

                //List<List<Segment>> segmentsPerLine = new();

                //while (cursorTop < containerRect.Bottom)
                //{
                //    var segments = GetEnclosedSegments(cursorTop, polygons, onlyAllowTextWhenAllPolygonsOverlaps);

                //    if (borderLeft)
                //    {
                //        CropSegmentsToBorder(segments, true, containerRect.Left);
                //    }
                //    if (borderRight)
                //    {
                //        CropSegmentsToBorder(segments, false, containerRect.Right);
                //    }

                //    segmentsPerLine.Add(segments);
                //    cursorTop += lineHeights;
                //}

                //// use segments to insert blocks
                //for (int line = 0; line < segmentsPerLine.Count - 1; line++)
                //{
                //    List<Segment> unifiedSegments = new();
                //    segmentedLines.Add(unifiedSegments);

                //    var currentSegments = segmentsPerLine[line];
                //    var nextSegments = segmentsPerLine[line + 1];
                //    int i = 0;
                //    int j = 0;
                //    float left, right;

                //    while (i < currentSegments.Count && j < nextSegments.Count)
                //    {
                //        var c = currentSegments[i];
                //        var n = nextSegments[j];
                //        if (c.Left >= n.Left)
                //        {
                //            // upper line starts further to the right
                //            //    ---
                //            // ---

                //            // check if lower lines segment includes the start of the upper line
                //            if (c.Left <= n.Right)
                //            {
                //                //  --
                //                // ---
                //                left = c.Left;

                //                if (c.Right <= n.Right)
                //                {
                //                    //  --
                //                    // ----
                //                    right = c.Right;
                //                    i++;
                //                }
                //                else
                //                {
                //                    //  --
                //                    // --
                //                    right = n.Right;
                //                    j++;
                //                }

                //                unifiedSegments.Add(new(left, right));
                //            }
                //            else
                //            {
                //                //    --
                //                // --
                //                j++;
                //            }
                //        }
                //        else
                //        {
                //            // upper line starts further to the left
                //            // ---
                //            //    ---

                //            if (c.Right >= n.Left)
                //            {
                //                // ---
                //                //  --
                //                left = n.Left;

                //                if (c.Right >= n.Right)
                //                {
                //                    // ----
                //                    //  --
                //                    right = n.Right;
                //                    j++;
                //                }
                //                else
                //                {
                //                    // --
                //                    //  --
                //                    right = c.Right;
                //                    i++;
                //                }
                //                unifiedSegments.Add(new(left, right));
                //            }
                //            else
                //            {
                //                // --
                //                //    --
                //                i++;
                //            }
                //        }
                //    }
                //}

                //IText myText;
                if (creator == null)
                {
                    //segmentedText = SplitTextBySegments(text, str => font.Value.MeasureString(str).X * FontScale.X, splitMethod,
                    //        lineHeights, textTop, segmentedLines, out overflow);
                }
                else
                {
                    // replace text with markup
                    SegmentedMarkup = new MarkupRoot(creator, text);
                    //myText = new MyMarkup(markup);
                    MarkupSettings settings = new(null, font, new Anchor(Vector2.Zero, anchor), Color.White, anchor.X, FontScale);
                    SplitMarkupBySegments(SegmentedMarkup, settings, splitMethod,
                            defaultLineHeight, textTop, textBottom, polygons, out overflow);

                    // find indices of spaces and \ns and seperations between f.ex. text and images
                    //markup.Root.Children

                }

                //totalSegmentsWidth = segmentedLines.Sum(f => f.Sum(g => g.Right - g.Left));
            }

            CreateAnchors(font);
        }

        private void CreateAnchors(Ref<SpriteFont> font)
        {
            //// create anchors for drawing later on
            //int segmentedTextIndex = 0;
            //float cursorTop1 = textTop;
            //for (int i = 0; i < segmentedLines.Count; i++)
            //{
            //    for (int j = 0; j < segmentedLines[i].Count; j++)
            //    {
            //        var s = segmentedLines[i][j];
            //        Anchor drawAnchor;
            //        if (globalAnchor)
            //        {
            //            // ---XXXXX|X---  
            //            // -> (when using global anchor of x=0.5)
            //            // ----XXX|XXX---
            //            float anchorXPos = containerRect.GetPos(anchor.X, 0f).X;
            //            float textWidth = font.Value.MeasureString(segmentedText[segmentedTextIndex]).X * FontScale.X;
            //            if (anchorXPos + textWidth * (1f - anchor.X) > s.Right)
            //            {
            //                drawAnchor = new Anchor(s.Right, cursorTop1, 1f, 0f);
            //            }
            //            else if (anchorXPos - textWidth * anchor.X < s.Left)
            //            {
            //                drawAnchor = new Anchor(s.Left, cursorTop1, 0f, 0f);

            //            }
            //            else
            //            {
            //                drawAnchor = new Anchor(anchorXPos, cursorTop1, anchor.X, 0f);
            //            }
            //        }
            //        else
            //        {
            //            drawAnchor = new Rect(s.Left, cursorTop1, s.Right - s.Left, lineHeights).GetAnchor(anchor.X, 0f);
            //        }
            //        s.Anchor = drawAnchor;

            //        segmentedTextIndex++;
            //        if (segmentedTextIndex >= segmentedText.Count)
            //        {
            //            return;
            //        }
            //    }
            //    cursorTop1 += lineHeights;
            //}
        }

        private void CropSegmentsToBorder(List<Segment> segments, bool left, float x)
        {
            if (left)
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    if (segments[i].Right < x)
                    {
                        segments.RemoveAt(i--);
                    }
                    else if (segments[i].Left < x)
                    {
                        segments[i] = new(x, segments[i].Right);
                    }
                }
            }
            else
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    if (segments[i].Left > x)
                    {
                        segments.RemoveAt(i--);
                    }
                    else if (segments[i].Right > x)
                    {
                        segments[i] = new(segments[i].Left, x);
                    }
                }
            }
        }

        class BlockList
        {
            List<Block> blocks;
        }

        class Block
        {
            float width;
            float height;
            float marginRight;
            float[] splits; // where one could split the block if he really wanted to
            Vector2 pos;
        }

        //interface IText
        //{
        //    string GetFullText();
        //    char Get(int index);
        //}

        //class MyString : IText
        //{
        //    private string text;

        //    public string GetFullText() => text;
        //    public char Get(int index) => text[index];

        //    public MyString(string text)
        //    {
        //        this.text = text;
        //    }
        //}

        //class MyMarkup : IText
        //{
        //    private readonly string fullText;
        //    private MarkupRoot markup;

        //    public string GetFullText() => fullText;

        //    public MyMarkup(string fullText, MarkupRoot markup)
        //    {
        //        this.fullText = fullText;
        //        this.markup = markup;
        //    }
        //}

        private List<string> SplitTextBySegments(string text, Func<string, float> measureFontWidth, PolygonTextSplit splitMethod, float lineSpacing, float textTop, List<List<Segment>> segmentsPerLine, out float overflow)
        {
            int segmentStart = 0;
            List<string> segmentedText = new();
            int i = 0, j = 0;

            // find first segment
            while (segmentsPerLine[i].Count == 0)
            {
                i++;
                if (i >= segmentsPerLine.Count)
                {
                    // no segments at all
                    string fullText = text;//.GetFullText();
                    segmentedText.Add(fullText);
                    overflow = measureFontWidth(fullText);
                    return segmentedText;
                }
            }

            float currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
            int lastSpaceIndex = -1;
            overflow = 0f;

            float lastMeasuredWidth = -1f;
            for (int textIndex = 0; textIndex < text.Length; textIndex++)
            {
                if (text[textIndex] == '\n')
                {
                    segmentedText.Add(text.Substring(segmentStart, textIndex - segmentStart));
                    segmentStart = textIndex + 1; // after \n
                    j++; // next segment

                    // skip all segments in the current line
                    while (j < segmentsPerLine[i].Count)
                    {
                        segmentedText.Add("");
                        j++;
                    }

                    if (!NextLine(ref overflow))
                    {
                        return segmentedText;
                    }
                    currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
                    continue;
                }
                else if (text[textIndex] == ' ')
                {
                    lastSpaceIndex = textIndex;
                    continue;
                }
                string segmentToAdd = text.Substring(segmentStart, textIndex + 1 - segmentStart);

                lastMeasuredWidth = measureFontWidth(segmentToAdd);
                if (lastMeasuredWidth > currentSegmentWidth)
                {
                    bool splitMidWord = splitMethod == PolygonTextSplit.AlwaysMidWord;
                    if (!splitMidWord)
                    {
                        if (lastSpaceIndex == -1)
                        {
                            if (splitMethod == PolygonTextSplit.AllowMidWordIfSpaceNotPossible && segmentToAdd.Length > 1)
                            {
                                splitMidWord = true;
                            }
                            else
                            {
                                // skip this section, as there's no space in the segment
                                segmentToAdd = "";
                                // back to the start of the text segment
                            }
                        }
                        else
                        {
                            segmentToAdd = text.Substring(segmentStart, lastSpaceIndex - segmentStart);
                            segmentStart = lastSpaceIndex + 1; // next segment starts after the last space
                            lastSpaceIndex = -1;
                        }
                    }
                    if (splitMidWord)
                    {
                        segmentToAdd = segmentToAdd.Remove(segmentToAdd.Length - 1);
                        segmentStart = textIndex;
                        lastSpaceIndex = -1;
                    }

                    segmentedText.Add(segmentToAdd);
                    textIndex = segmentStart - 1; // -1 because we add +1 add the end of the for loop
                    j++;
                    while (j >= segmentsPerLine[i].Count)
                    {
                        if (!NextLine(ref overflow))
                        {
                            return segmentedText;
                        }
                    }

                    currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
                }
            }

            segmentedText.Add(text.Substring(segmentStart));

            // underflow
            if (lastMeasuredWidth != -1f)
            {
                float underflow = currentSegmentWidth - lastMeasuredWidth;

                if (i < segmentsPerLine.Count)
                {
                    while (true)
                    {
                        j++;
                        if (j >= segmentsPerLine[i].Count)
                        {
                            j = 0;
                            i++;
                            if (i >= segmentsPerLine.Count)
                            {
                                break;
                            }
                        }

                        if (j < segmentsPerLine[i].Count)
                        {
                            underflow += segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
                        }
                    }
                }
                overflow = -underflow;
            }

            return segmentedText;

            bool NextLine(ref float overflow)
            {
                lastSpaceIndex = -1;
                j = 0;
                i++;
                if (i >= segmentsPerLine.Count)
                {
                    // we filled all segments, but there's still text missing
                    // simply append to the last segment
                    if (segmentStart >= 1 && segmentStart - 1 < text.Length && text[segmentStart - 1] == ' ')
                    {
                        segmentStart--;
                    }
                    string overflowText = text.Substring(segmentStart);
                    segmentedText[^1] += overflowText;
                    overflow = measureFontWidth(overflowText);
                    return false;
                }
                return true;
            }
        }

        private void SplitMarkupBySegments(MarkupRoot markup, MarkupSettings settings, PolygonTextSplit splitMethod, float defaultLineHeight, 
            float topY, float bottomY, List<List<Vector2>> polygons, out float overflow, float minX = -99999f, float maxX = 99999f, bool allowBreakBetweenTextAndTexture = true)
        {
            MarkupIndex segmentStart = new(markup.Root);
            float minimumLineHeightForThisLine = defaultLineHeight;
            Rect segment = new Rect(minX, topY,0,0);
            List<Rect> previousSegmentsThisLine = new();
            MarkupIndex? lastPossibleBreakIndex = null;
            bool isBreakChar = false;
            overflow = 0f;
            Vector2 previousTextSize = Vector2.Zero; // 0 0 means unset
            bool endOfContainerReached = false;
            bool lastSegmentInLine = false;

            for (MarkupIndex textIndex = segmentStart.Clone(); !endOfContainerReached && !textIndex.EndReached(); textIndex++)
            {
                bool manualNewLine = textIndex.CurrentNode is MarkupNewLine;
                Vector2 textSize = Vector2.Zero;

                if (manualNewLine)
                {
                    lastPossibleBreakIndex = textIndex.Clone();
                    isBreakChar = true;
                }
                else
                {
                    if (markup[textIndex] == ' ')
                    {
                        lastPossibleBreakIndex = textIndex.Clone();
                        isBreakChar = true;
                        continue;
                    }
                    if (allowBreakBetweenTextAndTexture
                        && textIndex.CurrentNode is MarkupTexture
                        && !textIndex.IsEqual(segmentStart))
                    {
                        lastPossibleBreakIndex = textIndex.Clone();
                        isBreakChar = false;
                    }
                    textSize = markup.GetSize(settings, segmentStart, textIndex + 1);

                    if (previousTextSize != Vector2.Zero && textSize.Y > previousTextSize.Y)
                    {
                        // text size increased!
                        // check if we can extend all segments in the current line downwards

                        List<Rect> segmentsThisLine = previousSegmentsThisLine.ToList();
                        segmentsThisLine.Add(segment);
                        float grow = textSize.Y - previousTextSize.Y;
                        bool allCanGrow = true;
                        foreach (var s in segmentsThisLine)
                        {
                            if (!CanGrow(s, grow))
                            {
                                allCanGrow = false;
                                break;
                            }

                            bool CanGrow(Rect segment, float grow)
                            {
                                return false;
                            }
                        }

                        if (allCanGrow)
                        {
                            // grow all
                            foreach (var s in segmentsThisLine)
                            {
                                s.Height = textSize.Y;
                            }
                            // TODO: also somehow grow the Jump() markups. This would require a new kind of JumpIntoRectangle markup?
                        }
                        else
                        {
                            // reposition with higher height or break to new segment
                            // remember beforehand
                            Rect rememberSegment = segment.CloneRect();
                            segment.Size = textSize;
                            if (!GetNextSegment())
                            {
                                // revert back to previous segment with to shallow size -> this will trigger a segment break
                                segment.Pos = rememberSegment.Pos;
                                segment.Size = rememberSegment.Size;
                            }
                        }

                    }
                    previousTextSize = textSize;

                    if (segment.Width == 0f)// is unset?
                    {
                        while (!endOfContainerReached)
                        {
                            // find current segment
                            // TODO: implement a border for minDistX in case of more than one segments per line (replace -9999999f)
                            segment.Size = textSize;

                            if (GetNextSegment())
                            {
                                break;
                            }
                            else
                            {
                                NewLine();
                            }
                        }
                    }
                }

                if (manualNewLine 
                    || textSize.X > segment.Width 
                    || textSize.Y > segment.Height)
                {
                    bool splitMidWord = splitMethod == PolygonTextSplit.AlwaysMidWord;
                    if (!splitMidWord)
                    {
                        if (lastPossibleBreakIndex == null)
                        {
                            // TODO
                            //if (splitMethod == PolygonTextSplit.AllowMidWordIfSpaceNotPossible) // TODO: && segmentCharCount > 1)
                            //{
                            //    splitMidWord = true;
                            //}
                            //else
                            {
                                // skip this section, as there's no space in the segment
                                markup.InsertJump(segmentStart, GetJumpVector(segmentStart), textIndex);
                                // back to the start of the text segment
                            }
                        }
                        else
                        {

                            markup.InsertJump(segmentStart, GetJumpVector(lastPossibleBreakIndex), lastPossibleBreakIndex, textIndex);
                            if (isBreakChar)
                            {
                                lastPossibleBreakIndex++;
                            }
                            segmentStart = lastPossibleBreakIndex; // next segment starts after the last space
                            lastPossibleBreakIndex = null;
                        }
                    }
                    if (splitMidWord)
                    {
                        lastPossibleBreakIndex = null;
                        markup.InsertJump(segmentStart, GetJumpVector(textIndex), textIndex);
                        segmentStart = textIndex.Clone();
                    }

                    CloseCurrentSegment();

                    if (manualNewLine || lastSegmentInLine)
                    {
                        NewLine();
                    }
                }
            }

            if (!endOfContainerReached)
            {
                segments.Add(segment.CloneRect());

                markup.InsertJump(segmentStart, GetJumpVector(null));
            }

            //// underflow
            //if (lastMeasuredWidth != -1f)
            //{
            //    float underflow = currentSegmentSize.X - lastMeasuredWidth;

            //    if (i < segmentsPerLine.Count)
            //    {
            //        while (true)
            //        {
            //            j++;
            //            if (j >= segmentsPerLine[i].Count)
            //            {
            //                j = 0;
            //                i++;
            //                if (i >= segmentsPerLine.Count)
            //                {
            //                    break;
            //                }
            //            }

            //            if (j < segmentsPerLine[i].Count)
            //            {
            //                underflow += segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
            //            }
            //        }
            //    }
            //    overflow = -underflow;
            //}

            void NewLine()
            {
                // no fitting segment found
                // try next line
                previousSegmentsThisLine.Clear();
                segment.Y += minimumLineHeightForThisLine;
                minimumLineHeightForThisLine = defaultLineHeight;
                segment.X = minX;

                // end reached?
                if (segment.Y + defaultLineHeight > bottomY)
                {
                    endOfContainerReached = true;
                }
            }

            Vector2 GetJumpVector(MarkupIndex? measureWidthUntil)
            {
                Vector2 jumpTo = segment.Pos;

                // anchor text inside segment, if anchor is not left aligned
                if (anchor.X != 0f && !segmentStart.IsEqual(measureWidthUntil))
                {
                    float textSegmentWidth = markup.GetSize(settings, segmentStart, measureWidthUntil == null ? null : (measureWidthUntil - 1)).X;
                    jumpTo.X += (segment.Width - textSegmentWidth) * anchor.X;
                }
                return jumpTo;
            }

            void CloseCurrentSegment()
            {
                var cloneSegment = segment.CloneRect();
                previousSegmentsThisLine.Add(cloneSegment);
                segments.Add(cloneSegment);
                segment.Left = segment.Right; // start next segment at the earliest of the right of the current segment
                segment.Width = 0f; // trigger getting next segment
                previousTextSize = Vector2.Zero; // new segment means, last text size can be ignored
                minimumLineHeightForThisLine = MathF.Max(minimumLineHeightForThisLine, segment.Height);
            }

            bool GetNextSegment()
            {
                if (minimumLineHeightForThisLine > segment.Height)
                {
                    segment.Height = minimumLineHeightForThisLine;
                }

                List<float> openX = new();
                List<float> closeX = new();
                foreach (var polygon in polygons)
                {
                    for (int p = 0; p < polygon.Count; p++)
                    {
                        int p2 = (p + 1) % polygon.Count;
                        Vector2 v1 = polygon[p];
                        Vector2 v2 = polygon[p2];
                        float xCollision;
                        float? xCollisionMaybe;

                        #region collision check

                        if (v1.Y < v2.Y)
                        {
                            // edge that closes the polygon
                            if (v2.Y < segment.Top || v1.Y > segment.Bottom)
                            {
                                // if v1 is above the cursor, v2 is too
                                continue;
                            }

                            if (v1.X <= v2.X)
                            {
                                // vertex 1 is more left than vertex 2
                                if (v1.Y >= segment.Top)
                                {
                                    // vertex 1 lies on same height as rect (vertex collision)
                                    xCollision = v1.X;
                                }
                                else
                                {
                                    if ((xCollisionMaybe = CheckEdgeCollision(segment.Top)) == null)
                                    {
                                        continue;
                                    }
                                    xCollision = xCollisionMaybe.Value;
                                }
                            }
                            else
                            {
                                // vertex 2 is more right than vertex 1
                                if (v2.Y <= segment.Bottom)
                                {
                                    // vertex 2 lies on same height as rect (vertex collision)
                                    xCollision = v2.X;
                                }
                                else
                                {
                                    if ((xCollisionMaybe = CheckEdgeCollision(segment.Bottom)) == null)
                                    {
                                        continue;
                                    }
                                    xCollision = xCollisionMaybe.Value;
                                }
                            }

                            if (xCollision < maxX)
                            {
                                closeX.Add(xCollision);
                            }
                        }
                        else
                        {
                            // edge that opens up the polygon

                            if (v1.Y < segment.Top || v2.Y > segment.Bottom)
                            {
                                // if v1 is above the cursor, v2 is too
                                continue;
                            }


                            if (v1.X >= v2.X)
                            {
                                // vertex 1 is more right than vertex 2
                                if (v1.Y <= segment.Bottom)
                                {
                                    // vertex 1 lies on same height as rect (vertex collision)
                                    xCollision = v1.X;
                                }
                                else
                                {
                                    // only check edge collision with bottom of rect
                                    if ((xCollisionMaybe = CheckEdgeCollision(segment.Bottom)) == null)
                                    {
                                        continue;
                                    }
                                    xCollision = xCollisionMaybe.Value;
                                }
                            }
                            else
                            {
                                // vertex 2 is more right than vertex 1
                                if (v2.Y >= segment.Top)
                                {
                                    // vertex 2 lies on same height as rect (vertex collision)
                                    xCollision = v2.X;
                                }
                                else
                                {
                                    // only check edge collision with top of rect
                                    if ((xCollisionMaybe = CheckEdgeCollision(segment.Top)) == null)
                                    {
                                        continue;
                                    }
                                    xCollision = xCollisionMaybe.Value;
                                }
                            }

                            if (xCollision >= segment.X)
                            {
                                openX.Add(xCollision);
                            }
                        }

                        float? CheckEdgeCollision(float cursorTopOrBottom)
                        {
                            // only check edge collision with bottom of rect
                            float distY = cursorTopOrBottom - v1.Y;
                            Vector2 edgeDir = v2 - v1;
                            float onLineLerp = distY / edgeDir.Y;
                            if (onLineLerp > 1f || onLineLerp < 0f)
                            {
                                // no collision
                                return null;
                            }
                            return v1.X + edgeDir.X * onLineLerp;
                        }

                        #endregion
                    }
                }

                openX.Sort();
                closeX.Sort();


                // iterate through all open positions and check if they collide with the next open or closed position
                // check what's the first distance to actually fit the current part in (lastMeasuredSize)
                while (openX.Count > 0)
                {
                    // remove all closes that are left to opens
                    while (closeX.Count > 0 && closeX[0] < openX[0])
                    {
                        closeX.RemoveAt(0);
                    }

                    // skip this open, if the next open is nearer than the next close
                    if (openX.Count > 1 && closeX.Count > 0 && openX[1] <= closeX[0])
                    {
                        openX.RemoveAt(0);
                        continue;
                    }

                    segment.X = openX[0];
                    if (closeX.Count > 0 && closeX[0] < segment.Right)
                    {
                        // cursor collides with close edge
                        openX.RemoveAt(0);
                    }
                    else
                    {
                        // no collision happened, take the open
                        break;
                    }
                }
                // remember to reuse left openX and closeX for next segment in current line (if line height doesn't change)
                if (openX.Count == 0)
                {
                    return false;
                }
                else
                {
                    lastSegmentInLine = openX.Count == 1;

                    float closeX1 = closeX.Count == 0 ? maxX : closeX[0];

                    segment.Width = closeX1 - openX[0];
                    return true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.Font != null && style.FontColor != null)
            {
                Draw(spriteBatch, style.Font, style.FontColor.Value);
            }
            if (style.FontBold != null && style.FontBoldColor != null)
            {
                Draw(spriteBatch, style.FontBold, style.FontBoldColor.Value);
            }
        }
        public void Draw(SpriteBatch spriteBatch, Ref<SpriteFont> font, Color color)
        {
            float cursorTop;
            int segmentedTextIndex = 0;
            cursorTop = textTop;
            //for (int i = 0; i < segmentedLines.Count; i++)
            //{
            //    for (int j = 0; j < segmentedLines[i].Count; j++)
            //    {
            //        var s = segmentedLines[i][j];
            //        font.Value.Draw(spriteBatch, segmentedText[segmentedTextIndex], s.Anchor!, color, FontScale);


            //        segmentedTextIndex++;
            //        if (segmentedTextIndex >= segmentedText.Count)
            //        {
            //            return;
            //        }
            //    }
            //    cursorTop += lineHeights;
            //}
        }

        public void DrawSegments(SpriteBatch spriteBatch, Color color)
        {
            DrawSegments(spriteBatch, segments, color);
        }

        public static void DrawSegments(SpriteBatch spriteBatch, List<Rect> segments, Color color)
        {
            foreach (var item in segments)
            {
                item.Draw(spriteBatch, color);
            }
        }

        //internal string GetSegmentedMarkupText()
        //{
        //    float cursorTop;
        //    int segmentedTextIndex = 0;
        //    cursorTop = textTop;
        //    string text = "";
        //    Vector2 offset = Vector2.Zero;
        //    for (int i = 0; i < unifiedSegmentsPerLine.Count; i++)
        //    {
        //        for (int j = 0; j < unifiedSegmentsPerLine[i].Count; j++)
        //        {
        //            var s = unifiedSegmentsPerLine[i][j];

        //            Int2 jump = new Int2((int)MathF.Round(offset.X + s.Left), (int)MathF.Round(cursorTop)); // round so no weird pixel smoothing happens on 0.5 (though sometimes this could be wanted. If so, make this optional)
        //            text += $"#jump({jump.X}|{jump.Y})" + segmentedText[segmentedTextIndex];

        //            segmentedTextIndex++;
        //            if (segmentedTextIndex >= segmentedText.Count)
        //            {
        //                return text;
        //            }
        //        }
        //        cursorTop += lineSpacing;
        //    }

        //    return text;
        //}

        public class Segment(float left, float right)
        {
            public float Left = left;
            public float Right = right;
            public Anchor? Anchor { get; set; }
        }

        class SegmentedLine
        {
            List<Segment> Segments { get; }
            public float LineHeight { get; }
        }

    }

    public enum PolygonTextSplit
    {
        OnlyOnSpace,
        AllowMidWordIfSpaceNotPossible,
        AlwaysMidWord,
    }
}