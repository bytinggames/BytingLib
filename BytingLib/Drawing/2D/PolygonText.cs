using BytingLib.Markup;
using BytingLib.UI;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Core;
using static System.Net.Mime.MediaTypeNames;

namespace BytingLib
{
    public class PolygonText
    {
        public Vector2 FontScale { get; private set; }
        private readonly List<string> segmentedText;
        public MarkupRoot? SegmentedMarkup { get; } = null;
        private readonly List<List<Segment>> unifiedSegmentsPerLine;
        private readonly float textTop;
        private readonly Rect containerRect;
        private readonly Vector2 anchor;
        private readonly bool globalAnchor;
        private readonly float lineSpacing;

        public PolygonText(string text, Ref<SpriteFont> font, Rect containerRect, Vector2 anchor, bool globalAnchor, List<List<Vector2>> polygons, PolygonTextSplit splitMethod,
            bool onlyAllowTextWhenAllPolygonsOverlaps = false, bool borderLeft = true, bool borderRight = true, Creator? creator = null)
        {
            this.containerRect = containerRect;
            this.anchor = anchor;
            this.globalAnchor = globalAnchor;
            FontScale = Vector2.One;
            textTop = 0f;
            unifiedSegmentsPerLine = new();
            segmentedText = new();

            float overflow = float.PositiveInfinity;
            float totalSegmentsWidth = 0f;
            Vector2? minFontScale = null;
            Vector2? maxFontScale = null;

            bool incrementedLines = false;

            lineSpacing = font.Value.LineSpacing * FontScale.Y;
            int lines = (int)MathF.Floor(containerRect.Height / lineSpacing);

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

                        lineSpacing = font.Value.LineSpacing * FontScale.Y;
                        lines = (int)MathF.Floor(containerRect.Height / lineSpacing);
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

                unifiedSegmentsPerLine.Clear();
                segmentedText.Clear();

                Vector2 anchorPos = containerRect.GetPos(anchor);


                //spriteBatch.DrawLine(new Vector2(minX, anchorPos.Y), new Vector2(maxX, anchorPos.Y), Color.Red);

                float totalTextHeight = lineSpacing * lines;
                textTop = anchorPos.Y - totalTextHeight * anchor.Y;
                float cursorTop = textTop;

                List<List<Segment>> segmentsPerLine = new();

                for (int line = 0; line <= lines; line++)
                {
                    var segments = GetEnclosedSegments(cursorTop, polygons, onlyAllowTextWhenAllPolygonsOverlaps);

                    if (borderLeft)
                    {
                        CropSegmentsToBorder(segments, true, containerRect.Left);
                    }
                    if (borderRight)
                    {
                        CropSegmentsToBorder(segments, false, containerRect.Right);
                    }

                    segmentsPerLine.Add(segments);
                    cursorTop += lineSpacing;
                }

                // use segments to insert blocks
                for (int line = 0; line < lines; line++)
                {
                    List<Segment> unifiedSegments = new();
                    unifiedSegmentsPerLine.Add(unifiedSegments);

                    var currentSegments = segmentsPerLine[line];
                    var nextSegments = segmentsPerLine[line + 1];
                    int i = 0;
                    int j = 0;
                    float left, right;

                    while (i < currentSegments.Count && j < nextSegments.Count)
                    {
                        var c = currentSegments[i];
                        var n = nextSegments[j];
                        if (c.Left >= n.Left)
                        {
                            // upper line starts further to the right
                            //    ---
                            // ---

                            // check if lower lines segment includes the start of the upper line
                            if (c.Left <= n.Right)
                            {
                                //  --
                                // ---
                                left = c.Left;

                                if (c.Right <= n.Right)
                                {
                                    //  --
                                    // ----
                                    right = c.Right;
                                    i++;
                                }
                                else
                                {
                                    //  --
                                    // --
                                    right = n.Right;
                                    j++;
                                }

                                unifiedSegments.Add(new(left, right));
                            }
                            else
                            {
                                //    --
                                // --
                                j++;
                            }
                        }
                        else
                        {
                            // upper line starts further to the left
                            // ---
                            //    ---

                            if (c.Right >= n.Left)
                            {
                                // ---
                                //  --
                                left = n.Left;

                                if (c.Right >= n.Right)
                                {
                                    // ----
                                    //  --
                                    right = n.Right;
                                    j++;
                                }
                                else
                                {
                                    // --
                                    //  --
                                    right = c.Right;
                                    i++;
                                }
                                unifiedSegments.Add(new(left, right));
                            }
                            else
                            {
                                // --
                                //    --
                                i++;
                            }
                        }
                    }
                }

                //IText myText;
                if (creator == null)
                {
                    segmentedText = SplitTextBySegments(text, str => font.Value.MeasureString(str).X * FontScale.X, splitMethod,
                            lineSpacing, textTop, unifiedSegmentsPerLine, out overflow);
                }
                else
                {
                    // replace text with markup
                    SegmentedMarkup = new MarkupRoot(creator, text);
                    //myText = new MyMarkup(markup);
                    MarkupSettings settings = new(null, font, new Anchor(Vector2.Zero, anchor), Color.White, anchor.X, FontScale);
                    SplitMarkupBySegments(SegmentedMarkup, settings, splitMethod,
                            lineSpacing, textTop, unifiedSegmentsPerLine, out overflow);

                    // find indices of spaces and \ns and seperations between f.ex. text and images
                    //markup.Root.Children

                }

                totalSegmentsWidth = unifiedSegmentsPerLine.Sum(f => f.Sum(g => g.Right - g.Left));
            }

            CreateAnchors(font);
        }

        private void CreateAnchors(Ref<SpriteFont> font)
        {
            // create anchors for drawing later on
            int segmentedTextIndex = 0;
            float cursorTop1 = textTop;
            for (int i = 0; i < unifiedSegmentsPerLine.Count; i++)
            {
                for (int j = 0; j < unifiedSegmentsPerLine[i].Count; j++)
                {
                    var s = unifiedSegmentsPerLine[i][j];
                    Anchor drawAnchor;
                    if (globalAnchor)
                    {
                        // ---XXXXX|X---  
                        // -> (when using global anchor of x=0.5)
                        // ----XXX|XXX---
                        float anchorXPos = containerRect.GetPos(anchor.X, 0f).X;
                        float textWidth = font.Value.MeasureString(segmentedText[segmentedTextIndex]).X * FontScale.X;
                        if (anchorXPos + textWidth * (1f - anchor.X) > s.Right)
                        {
                            drawAnchor = new Anchor(s.Right, cursorTop1, 1f, 0f);
                        }
                        else if (anchorXPos - textWidth * anchor.X < s.Left)
                        {
                            drawAnchor = new Anchor(s.Left, cursorTop1, 0f, 0f);

                        }
                        else
                        {
                            drawAnchor = new Anchor(anchorXPos, cursorTop1, anchor.X, 0f);
                        }
                    }
                    else
                    {
                        drawAnchor = new Rect(s.Left, cursorTop1, s.Right - s.Left, lineSpacing).GetAnchor(anchor.X, 0f);
                    }
                    s.Anchor = drawAnchor;

                    segmentedTextIndex++;
                    if (segmentedTextIndex >= segmentedText.Count)
                    {
                        return;
                    }
                }
                cursorTop1 += lineSpacing;
            }
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
            //while (text.Iterate())
            //{

            //}
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

        private void SplitMarkupBySegments(MarkupRoot markup, MarkupSettings settings, PolygonTextSplit splitMethod, float lineSpacing, float textTop, List<List<Segment>> segmentsPerLine, out float overflow)
        {
            MarkupIndex segmentStart = new(markup.Root);
            int i = 0, j = 0;

            // find first segment
            while (segmentsPerLine[i].Count == 0)
            {
                i++;
                if (i >= segmentsPerLine.Count)
                {
                    // no segments at all
                    // let the markup as it is
                    overflow = markup.GetSize(settings).X;
                    return;
                }
            }

            float currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
            MarkupIndex? lastSpaceIndex = null;
            overflow = 0f;

            float lastMeasuredWidth = -1f;
            Vector2 lastSegmentStartPos = new Vector2(segmentsPerLine[i][j].Left, textTop);
            //Rect lastSegmentStartPos = new Rect(segmentsPerLine[i][j].Left, textTop, segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left, lineSpacing /* todo */);

            for (MarkupIndex textIndex = segmentStart.Clone(); !(textIndex + 1).EndReached(); textIndex++)
            {
                //if (markup[textIndex] == '\n') // not sure if this is necessary. aren't \ns replaced with MarkupNewLine()
                //{
                //    markup.InsertMove(segmentStart, GetMoveVector());
                //    segmentStart = textIndex + 1; // after \n
                //    j++; // next segment

                //    // skip all segments in the current line
                //    while (j < segmentsPerLine[i].Count)
                //    {
                //        segmentedText.Add("");
                //        j++;
                //    }

                //    if (!NextLine(ref overflow))
                //    {
                //        return;
                //    }
                //    currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
                //    continue;
                //}
                //else 
                if (markup[textIndex] == ' ')
                {
                    lastSpaceIndex = textIndex.Clone(); // clone if not a struct
                    continue;
                }
                int segmentCharCount = 0;// segmentStart.CharacterCountTo(textIndex + 1);
                lastMeasuredWidth = markup.GetSize(settings, segmentStart, textIndex + 1).X;
                if (lastMeasuredWidth > currentSegmentWidth)
                {
                    bool splitMidWord = splitMethod == PolygonTextSplit.AlwaysMidWord;
                    if (!splitMidWord)
                    {
                        if (lastSpaceIndex == null)
                        {
                            if (splitMethod == PolygonTextSplit.AllowMidWordIfSpaceNotPossible && segmentCharCount > 1)
                            {
                                splitMidWord = true;
                            }
                            else
                            {
                                // skip this section, as there's no space in the segment
                                markup.InsertJump(segmentStart, GetJumpVector(segmentStart), textIndex);
                                // back to the start of the text segment
                            }
                        }
                        else
                        {
                            markup.InsertJump(segmentStart, GetJumpVector(lastSpaceIndex + 1), lastSpaceIndex, textIndex);
                            segmentStart = lastSpaceIndex + 1; // next segment starts after the last space
                            lastSpaceIndex = null;
                        }
                    }
                    if (splitMidWord)
                    {
                        lastSpaceIndex = null;
                        markup.InsertJump(segmentStart, GetJumpVector(textIndex), textIndex);
                        segmentStart = textIndex.Clone();
                    }

                    textIndex = segmentStart - 1; // -1 because we add +1 add the end of the for loop
                    j++;
                    while (j >= segmentsPerLine[i].Count)
                    {
                        if (!NextLine(ref overflow))
                        {
                            return;
                        }
                    }

                    currentSegmentWidth = segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left;
                }
            }

            markup.InsertJump(segmentStart, GetJumpVector(null));

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

            Vector2 GetJumpVector(MarkupIndex? measureWidthUntil)
            {
                // figure out where last segment left off
                //Vector2 lastSegmentEnd = lastSegmentStartPos + new Vector2(lastMeasuredWidth, 0f);
                // figure out where new segment starts
                Vector2 newStart = new Vector2(segmentsPerLine[i][j].Left, textTop + i * lineSpacing);
                //Vector2 move = newStart - lastSegmentEnd;
                lastSegmentStartPos = newStart;


                //Rect newStart = new Rect(segmentsPerLine[i][j].Left, textTop + i * lineSpacing /* todo */, segmentsPerLine[i][j].Right - segmentsPerLine[i][j].Left, lineSpacing /* todo */);
                //Vector2 move = newStart - lastSegmentEnd;
                lastSegmentStartPos = newStart;

                if (anchor.X != 0f)
                {
                    float textSegmentWidth = markup.GetSize(settings, segmentStart, measureWidthUntil == null ? null : (measureWidthUntil - 1)).X;
                    lastSegmentStartPos.X += (currentSegmentWidth - textSegmentWidth) * anchor.X;
                }
                return lastSegmentStartPos;
                //return new Vector2(segmentsPerLine[i][j].Left, textTop);
            }

            bool NextLine(ref float overflow)
            {
                lastSpaceIndex = null;
                j = 0;
                i++;
                if (i >= segmentsPerLine.Count)
                {
                    // we filled all segments, but there's still text missing
                    // simply append to the last segment
                    if (!segmentStart.AtStart() && !(segmentStart - 1).EndReached() && markup[segmentStart - 1] == ' ')
                    {
                        segmentStart--;
                    }

                    overflow = markup.GetSize(settings, segmentStart).X;
                    return false;
                }
                return true;
            }
        }

        static List<Segment> GetEnclosedSegments(float y, List<List<Vector2>> concavePolygons, bool onlyAllowTextWhenAllPolygonsOverlaps = false)
        {
            List<float> left = new();
            List<float> right = new();

            foreach (var polygon in concavePolygons)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    int j = (i + 1) % polygon.Count;
                    if (y <= polygon[i].Y && y >= polygon[j].Y)
                    {
                        // line is at collision height
                        float colX = GetXCollisionOnLine(y, polygon[i], polygon[j]);
                        left.Add(colX);
                    }
                    else if (y <= polygon[j].Y && y >= polygon[i].Y)
                    {
                        float colX = GetXCollisionOnLine(y, polygon[i], polygon[j]);
                        right.Add(colX);
                    }
                }
            }
            left.Sort();
            right.Sort();

            List<Segment> segments = new();

            int l = 0;
            int r = 0;
            int open = 0;
            float leftOnFirstOpen = 0f;
            int openGoal = onlyAllowTextWhenAllPolygonsOverlaps ? concavePolygons.Count - 1 : 0;
            while (r < right.Count)
            {
                bool nextIsLeft = l < left.Count /*&& r < right.Count*/ && left[l] < right[r];
                if (nextIsLeft)
                {
                    if (open == 0)
                    {
                        leftOnFirstOpen = left[l];
                    }
                    open++;
                    //if (open)
                    //{

                    //}
                    //else
                    //{
                    //}
                    l++;
                }
                else
                {
                    open--;
                    if (open == openGoal)
                    {
                        // close it
                        if (onlyAllowTextWhenAllPolygonsOverlaps)
                        {
                            segments.Add(new(left[l - 1], right[r]));
                        }
                        else
                        {
                            segments.Add(new(leftOnFirstOpen, right[r]));
                        }
                    }
                    else if (open < 0)
                    {
                        open = 0; // not sure if this is necessary
                    }
                    r++;
                }
            }
            return segments;
        }

        private static float GetXCollisionOnLine(float y, Vector2 a, Vector2 b)
        {
            Vector2 lineLength = a - b;
            float lerp = (y - b.Y) / lineLength.Y;
            float colX = b.X + lineLength.X * lerp;
            return colX;
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
            for (int i = 0; i < unifiedSegmentsPerLine.Count; i++)
            {
                for (int j = 0; j < unifiedSegmentsPerLine[i].Count; j++)
                {
                    var s = unifiedSegmentsPerLine[i][j];
                    font.Value.Draw(spriteBatch, segmentedText[segmentedTextIndex], s.Anchor!, color, FontScale);


                    segmentedTextIndex++;
                    if (segmentedTextIndex >= segmentedText.Count)
                    {
                        return;
                    }
                }
                cursorTop += lineSpacing;
            }
        }

        public void DrawSegments(SpriteBatch spriteBatch, Color color)
        {
            DrawSegments(spriteBatch, lineSpacing, textTop, unifiedSegmentsPerLine, true, color);
        }

        public static float DrawSegments(SpriteBatch spriteBatch, float lineSpacing, float cursorTop, List<List<Segment>> segmentsPerLine, bool block, Color color)
        {
            foreach (var line in segmentsPerLine)
            {
                foreach (var segment in line)
                {
                    if (block)
                    {
                        spriteBatch.DrawRectangle(new(segment.Left, cursorTop, segment.Right - segment.Left, lineSpacing), color);
                    }
                    else
                    {
                        spriteBatch.DrawLine(new(segment.Left, cursorTop), new(segment.Right, cursorTop), color);
                    }
                }

                cursorTop += lineSpacing;
            }

            return cursorTop;
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
    }

    public enum PolygonTextSplit
    {
        OnlyOnSpace,
        AllowMidWordIfSpaceNotPossible,
        AlwaysMidWord,
    }
}