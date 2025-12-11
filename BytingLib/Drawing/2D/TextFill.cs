using BytingLib.Markup;

namespace BytingLib
{
    public class TextFill
    {
        public Vector2 FontScale { get; private set; }
        public MarkupRoot? SegmentedMarkup { get; } = null;
        private readonly float textTop;
        private readonly Vector2 anchor;
        private readonly bool globalAnchor;
        private readonly float anchorInLineY;
        List<Rect> segments = new();

        public TextFill(string text, Ref<SpriteFont> font, Rect containerRect, Vector2 anchor, bool globalAnchor, List<List<Vector2>> polygons, TextWrap splitMethod, bool relativeJumps,
            bool borderLeft = true, bool borderRight = true, Creator? creator = null, bool iterative = true, float anchorInLineY = 0.5f, float minLineSpacing = 0f)
        {
            this.anchor = anchor;
            this.globalAnchor = globalAnchor;
            this.anchorInLineY = anchorInLineY;
            FontScale = Vector2.One;
            textTop = 0f;

            Vector2? minFontScale = null;
            Vector2? maxFontScale = null;

            bool endlessHeight = containerRect.Height <= 0f;

            if (endlessHeight && polygons.Count == 0 && splitMethod == TextWrap.OnlyOnSpace)
            {
                splitMethod = TextWrap.AllowMidWordIfSpaceNotPossible;
            }

            // in case the polygon has floating point inaccuracies which could prevent a line at the exact top (0.000) add a slight offset
            if (polygons.Count > 0)
            {
                float startOffset = 1f;
                containerRect.Y += startOffset;
                if (!endlessHeight)
                {
                    containerRect.Height -= startOffset;
                }
            }

            float defaultLineHeight = MathF.Max(minLineSpacing, font.Value.LineSpacing) * FontScale.Y;
            float textHeightEstimation = containerRect.Height;

            bool correctOverflow;
            int fontScaleIterations, yOffsetIterations;
            if (iterative)
            {
                correctOverflow = endlessHeight ? false : true;
                fontScaleIterations = endlessHeight ? 0 : 5;
                yOffsetIterations = anchor.Y == 0 ? 0 : 5;
            }
            else
            {
                correctOverflow = false;
                fontScaleIterations = 1;
                yOffsetIterations = 0;
            }
            for (int iteration = 0; iteration < fontScaleIterations + yOffsetIterations; iteration++)
            {
                Vector2 anchorPos = containerRect.GetPos(anchor);

                textTop = anchorPos.Y - textHeightEstimation * anchor.Y;
                float textBottom = endlessHeight ? float.PositiveInfinity : textTop + textHeightEstimation;


                if (creator == null)
                {
                    SegmentedMarkup = new MarkupRoot(new MarkupCollection(new MarkupText(text)));
                }
                else
                {
                    // replace text with markup
                    SegmentedMarkup = new MarkupRoot(creator, text);
                }

                MarkupSettings settings = new(null, font, new Anchor(Vector2.Zero, anchor), Color.White, anchor.X, FontScale)
                {
                    JumpOffset = relativeJumps ? containerRect.Pos : Vector2.Zero
                };
                // check if the markup is practically empty and won't draw anything anyways
                if (SegmentedMarkup.Root.IterateOverLeaves(settings).All(f => f.GetSize(settings) == Vector2.Zero))
                {
                    SegmentedMarkup = null;
                    return;
                }
                
                segments = SplitMarkupBySegments(SegmentedMarkup, settings, splitMethod,
                    defaultLineHeight, textTop, textBottom, polygons, out float overflowFract, borderLeft ? containerRect.Left : null, borderRight ? containerRect.Right : null);

                if (overflowFract <= -1f)
                {
                    // no content drawn
                    SegmentedMarkup = null;
                    return;
                }

                if (correctOverflow
                    && overflowFract > 0f
                    && (iteration + 1 == fontScaleIterations // was this the last font scale iteration?
                        || iteration + 1 == fontScaleIterations + yOffsetIterations)) // or was this the last yOffset iteration?
                {
                    iteration--; // retry until text doesn't overflow anymore
                }

                if (iteration + 1 < fontScaleIterations) // check if next iteration is still scaling the font
                {
                    // try font scaling
                    if (overflowFract < 0f)
                    {
                        minFontScale = FontScale;
                    }
                    else
                    {
                        maxFontScale = FontScale;
                    }

                    float scaleFontBy = 1f / (1f + overflowFract);
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

                    defaultLineHeight = MathF.Max(minLineSpacing, font.Value.LineSpacing) * FontScale.Y;
                }
                else
                {
                    if (overflowFract.NearlyEqual(0f, 0.001f))
                    {
                        // fits (nearly) perfectly :o
                        // no need to optimize further
                        break;
                    }

                    float scaleHeightBy = 1f + overflowFract;
                    textHeightEstimation *= scaleHeightBy;

                    if (textHeightEstimation > containerRect.Height)
                    {
                        textHeightEstimation = containerRect.Height;
                    }
                }
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

        private List<Rect> SplitMarkupBySegments(MarkupRoot markup, MarkupSettings settings, TextWrap splitMethod, float defaultLineHeight, 
            float topY, float bottomY, List<List<Vector2>> polygons, out float overflowFract, float? minX, float? maxX, bool allowBreakBetweenTextAndTexture = true)
        {
            List<Rect> segments = new List<Rect>();
            MarkupIndex segmentStart = new(markup.Root);
            float minimumLineHeightForThisLine = defaultLineHeight;
            Rect segment = new Rect(-float.MaxValue, topY,0,0);
            List<Rect> previousSegmentsThisLine = new();
            MarkupIndex? lastPossibleBreakIndex = null;
            MarkupIndex? lastPossibleMidWordBreakIndex = null;
            bool isBreakChar = false;
            overflowFract = 0f;
            Vector2 previousTextSize = Vector2.Zero; // 0 0 means unset
            bool endOfContainerReached = false;
            bool lastSegmentInLine = false;
            float widestSegmentThatWasToNarrow = 0f;

            for (MarkupIndex textIndex = segmentStart.Clone(); !endOfContainerReached && !textIndex.AtEnd(); textIndex++)
            {
                bool breakAllowed = !textIndex.selectedNodeHierarchy.Any(f => f is MarkupNoBreak);
                bool manualNewLine = textIndex.CurrentNode is MarkupNewLine;
                Vector2 textSize = Vector2.Zero;

                if (manualNewLine)
                {
                    lastPossibleMidWordBreakIndex = lastPossibleBreakIndex = textIndex.Clone();
                    isBreakChar = true;
                }
                else
                {
                    char? currentChar = markup[textIndex];
                    if (currentChar == ' ')
                    {
                        if (breakAllowed)
                        {
                            lastPossibleMidWordBreakIndex = lastPossibleBreakIndex = textIndex.Clone();
                            isBreakChar = true;
                        }
                        continue;
                    }
                    else if (breakAllowed && currentChar != null && CharacterAllowedAt.BeginningOfLine(currentChar.Value))
                    {
                        char? previousChar = markup[textIndex - 1];
                        if (previousChar != null && CharacterAllowedAt.EndOfLine(previousChar.Value))
                        {
                            lastPossibleMidWordBreakIndex = textIndex.Clone();
                        }
                    }

                    if (breakAllowed
                        && allowBreakBetweenTextAndTexture
                        && textIndex.CurrentNode is MarkupTexture
                        && !textIndex.IsEqual(segmentStart))
                    {
                        lastPossibleMidWordBreakIndex = lastPossibleBreakIndex = textIndex.Clone();
                        isBreakChar = false;
                    }
                    textSize = markup.GetSizeSubstring(settings, segmentStart, textIndex + 1);

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
                    bool splitMidWord = breakAllowed 
                        && splitMethod == TextWrap.AlwaysMidWord
                        && lastPossibleMidWordBreakIndex != null;
                    if (!splitMidWord)
                    {
                        if (lastPossibleBreakIndex == null)
                        {
                            if (breakAllowed
                                && splitMethod == TextWrap.AllowMidWordIfSpaceNotPossible 
                                && lastPossibleMidWordBreakIndex != null)
                            {
                                splitMidWord = true;

                                if (textIndex.IsEqual(segmentStart))
                                {
                                    // what if nothing is selected? a big image f.ex.? if textIndex == segmentIndex? and we have endless height? 
                                    // then simply add the image, even though it's overflowing (TODO: should be warned though by returning an overflow?)
                                    if (bottomY == float.PositiveInfinity)
                                    {
                                        textIndex++;
                                    }
                                }
                            }
                            else
                            {
                                // skip this section, as there's no space in the segment
                                markup.InsertJump(segmentStart, GetJumpVector(segmentStart), textIndex);
                                lastPossibleMidWordBreakIndex = null;
                            }
                        }
                        else
                        {
                            if (isBreakChar && lastPossibleBreakIndex.CurrentNode is MarkupText markupText)
                            {
                                // remove break char
                                markupText.Text = markupText.Text.Remove(lastPossibleBreakIndex.indexInString, 1);
                                if (textIndex.CurrentNode == lastPossibleBreakIndex.CurrentNode)
                                {
                                    textIndex--; // move text index one back, as a space in that string has been removed
                                }
                            }
                            else if (textIndex.CurrentNode is MarkupNewLine markupNewLine)
                            {
                                // remove MarkupNewLine
                                MarkupCollection parent = (MarkupCollection)textIndex.selectedNodeHierarchy[^2];
                                int newLineIndex = parent.Children.IndexOf(markupNewLine);
                                bool startAtCurrentIndex = segmentStart.IsEqual(textIndex);
                                textIndex++; // move after markup new line to remove it
                                parent.Children.RemoveAt(newLineIndex);
                                if (startAtCurrentIndex)
                                {
                                    segmentStart = textIndex.Clone();
                                }
                                lastPossibleBreakIndex = textIndex.Clone();
                            }


                            lastPossibleMidWordBreakIndex = null;
                            markup.InsertJump(segmentStart, GetJumpVector(lastPossibleBreakIndex), lastPossibleBreakIndex, textIndex);
                            //if (isBreakChar)
                            //{
                            //    lastPossibleBreakIndex++;
                            //    textIndex++;
                            //}
                            segmentStart = lastPossibleBreakIndex; // next segment starts after the last space
                            lastPossibleBreakIndex = null;
                        }
                    }
                    if (splitMidWord && lastPossibleMidWordBreakIndex != null)
                    {
                        lastPossibleBreakIndex = null;
                        markup.InsertJump(segmentStart, GetJumpVector(lastPossibleMidWordBreakIndex), lastPossibleMidWordBreakIndex, textIndex);
                        segmentStart = lastPossibleMidWordBreakIndex;
                        lastPossibleMidWordBreakIndex = null;
                    }
                    textIndex--; // because this gets incremented by this for loop, and we should still test the same index next segment

                    CloseCurrentSegment();

                    if (manualNewLine || lastSegmentInLine)
                    {
                        NewLine();

                        if (manualNewLine)
                        {
                            // in case multiple manual new lines are followed by each other, we need to reset the segment height, as that won't be updated otherwise (see above, we don't get a new segment if it's a newline)
                            segment.Height = defaultLineHeight;
                        }
                    }
                }
            }

            if (!endOfContainerReached)
            {
                segments.Add(segment.CloneRect());

                markup.InsertJump(segmentStart, GetJumpVector(null));
            }

            // check overflow / underflow (only if the region is actually limited and not infinite
            if (float.IsFinite(topY) && float.IsFinite(bottomY))
            {
                if (endOfContainerReached)
                {
                    // overflow
                    // measure current line size
                    Vector2 textSize = markup.GetSizeSubstring(settings, segmentStart);

                    float overflowWidth = textSize.X;

                    float totalSegmentsWidth;
                    //if (segments.Count > 0)
                    {
                        totalSegmentsWidth = segments.Sum(f => f.Width);
                    }
                    ////else if (minX != null && maxX != null)
                    ////{
                    ////    segment
                    ////    totalSegmentsWidth = 
                    //}
                    //totalSegmentsWidth += segment.Width;
                    totalSegmentsWidth += widestSegmentThatWasToNarrow;

                    overflowFract = overflowWidth / totalSegmentsWidth;
                    // yes, overflowFract can get infinite here, and it's supposed to
                }
                else
                {
                    // underflow
                    float heightTakenUpFract = (segment.Bottom - topY) / (bottomY - topY);
                    overflowFract = heightTakenUpFract - 1f;
                }
            }
            else
            {
                overflowFract = 0f; // no overflow as region is infinite
            }

            return segments;

            void NewLine()
            {
                // no fitting segment found
                // try next line
                previousSegmentsThisLine.Clear();
                segment.Y += minimumLineHeightForThisLine;
                minimumLineHeightForThisLine = defaultLineHeight;
                segment.X = -float.MaxValue;

                // end reached?
                if (segment.Y + defaultLineHeight > bottomY)
                {
                    endOfContainerReached = true;
                }
            }

            Vector2 GetJumpVector(MarkupIndex? measureWidthUntil)
            {
                Vector2 jumpTo = segment.Pos;
                jumpTo -= settings.JumpOffset;

                // anchor text inside segment, if anchor is not left aligned
                if (!segmentStart.IsEqual(measureWidthUntil) && (anchor.X != 0 || anchorInLineY != 0))
                {
                    Vector2 textSegmentSize = markup.GetSizeSubstring(settings, segmentStart, measureWidthUntil);
                    if (anchor.X != 0f)
                    {
                        jumpTo.X += (segment.Width - textSegmentSize.X) * anchor.X;
                    }
                    if (anchorInLineY != 0f)
                    {
                        jumpTo.Y += segment.Height * anchorInLineY;
                    }
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
                if (polygons.Count == 0)
                {
                    if (minX.HasValue)
                    {
                        openX.Add(minX.Value);
                    }
                    else
                    {
                        throw new Exception("text fill has no polygon and no minX set. No text can be inserted");
                    }

                    if (maxX.HasValue)
                    {
                        closeX.Add(maxX.Value);
                    }
                }
                else
                {
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

                                if (minX.HasValue && xCollision < minX.Value)
                                {
                                    xCollision = minX.Value;
                                }
                                if (xCollision >= segment.X
                                    && (!maxX.HasValue || xCollision < maxX.Value)) // no reason in opening up beyond the most far right x
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
                }

                openX.Sort();
                closeX.Sort();

                widestSegmentThatWasToNarrow = 0f;

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
                        widestSegmentThatWasToNarrow = float.Max(closeX[0] - segment.X, widestSegmentThatWasToNarrow);
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
                if (openX.Count == 0 || (closeX.Count == 0 && maxX == null))
                {
                    return false;
                }
                else
                {
                    lastSegmentInLine = openX.Count == 1;

                    float closeX1 = closeX.Count == 0 ? maxX!.Value : closeX[0];

                    segment.Width = closeX1 - openX[0];
                    return true;
                }
            }
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
    }

    public enum TextWrap
    {
        OnlyOnSpace,
        AllowMidWordIfSpaceNotPossible,
        AlwaysMidWord,
    }
}