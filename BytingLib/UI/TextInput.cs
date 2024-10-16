﻿namespace BytingLib.UI
{
    public class TextInput : Label
    {
        private KeyInputString? keyInputString;
        private bool cursorChanged;
        private Rect? cursorDraw;
        private List<Rect>? selectDraw;
        private bool mouseClick, mouseHold;
        private Vector2 mousePos;
        private bool catched;
        private bool drawFocused;
        float lastMSCursorOrSelectionChanged;
        private readonly GameSpeed updateSpeed;
        private readonly Predicate<char>? validateChar;
        private int? moveCursorVertically;
        public event Func<bool>? OnEnter;
        public KeyInputString.AllowNewLine AllowNewLine { get; }
        private bool doFocus;
        private bool doSelectAllOnClick;

        public TextInput(GameSpeed updateSpeed, string text = "", float width = 0, float height = 0, KeyInputString.AllowNewLine allowNewLine = KeyInputString.AllowNewLine.Never, Predicate<char>? validateChar = null)
            : base(text, width, height, false)
        {
            this.updateSpeed = updateSpeed;
            AllowNewLine = allowNewLine;
            this.validateChar = validateChar;
            lastMSCursorOrSelectionChanged = updateSpeed.TotalMSF();
        }

        protected override void UpdateTreeInner(Rect rect)
        {
            base.UpdateTreeInner(rect);

            cursorChanged = true; // force cursor draw update
        }

        protected override void UpdateSelf(ElementInput input)
        {
            if (input.FocusElement == this)
            {
                // update focused
                if (keyInputString == null)
                {
                    OnFocus(input);
                }
            }
            else
            {
                // update unfocused
                if (keyInputString != null)
                {
                    OnUnfocus();
                }
            }

            if (input.Mouse.Left.Pressed || doFocus)
            {
                bool hover = doFocus || AbsoluteRect.CollidesWith(input.Mouse.Position);
                doFocus = false;
                if (input.FocusElement == this)
                {
                    if (!hover)
                    {
                        input.FocusElement = null;
                        OnUnfocus();
                    }
                }
                else
                {
                    if (hover)
                    {
                        input.FocusElement = this;
                        OnFocus(input);
                    }
                }

                if (hover)
                {
                    mouseClick = true;
                    mousePos = input.Mouse.Position;
                    catched = true;
                    input.SetUpdateCatch(this);
                }
            }
            else if (catched)
            {
                mouseClick = false;

                if (input.Mouse.Left.Down)
                {
                    mouseHold = true;
                    mousePos = input.Mouse.Position;
                }
                else
                {
                    input.SetUpdateCatch(null);
                    catched = false;
                }
            }

            drawFocused = input.FocusElement == this;

            base.UpdateSelf(input);
        }

        private void OnFocus(ElementInput input)
        {
            keyInputString = new KeyInputString(input.Window, AllowNewLine);
            keyInputString.InputString = new InputString();
            keyInputString.InputString.Text = Text;
            keyInputString.InputString.OnTextChange += InputString_OnTextChange;
            keyInputString.InputString.OnCursorMoveOrSelectChanged += InputString_OnCursorMoveOrSelectChanged;
            keyInputString.InputString.MoveCursorVertically += MoveCursorVertically;
            keyInputString.OnEnter = () => OnEnter == null ? true : OnEnter();
            keyInputString.InputString.ValidateChar = validateChar;

            InputString_OnTextChange(keyInputString.InputString);
            InputString_OnCursorMoveOrSelectChanged(keyInputString.InputString);
        }

        private void InputString_OnCursorMoveOrSelectChanged(InputString inputString)
        {
            cursorChanged = true;

            lastMSCursorOrSelectionChanged = updateSpeed.TotalMSF();
        }

        private void InputString_OnTextChange(InputString inputString)
        {
            cursorChanged = true;
            Text = inputString.Text;
        }

        private void MoveCursorVertically(int byLineAmount)
        {
            moveCursorVertically = byLineAmount;
        }

        private void OnUnfocus()
        {
            keyInputString?.Dispose();
            keyInputString = null;
        }

        protected override void DisposeSelf()
        {
            keyInputString?.Dispose();

            base.DisposeSelf();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (mouseClick)
            {
                mouseClick = false;
                // set cursor to mouse click position
                Vector2 relativeMousePos = GlobalToLocalSpace(style, mousePos);
                keyInputString!.InputString!.Cursor = DrawSpaceToTextSpace(relativeMousePos, style);
                keyInputString!.InputString!.SelectStart = null;

                if (doSelectAllOnClick && keyInputString?.InputString != null)
                {
                    doSelectAllOnClick = false;
                    keyInputString?.InputString.SelectAll();
                }
            }
            if (mouseHold)
            {
                mouseHold = false;
                Vector2 relativeMousePos = GlobalToLocalSpace(style, mousePos);
                int selectPos = DrawSpaceToTextSpace(relativeMousePos, style);
                keyInputString!.InputString!.SelectStart = selectPos == keyInputString!.InputString!.Cursor ? null : selectPos;
            }

            if (moveCursorVertically != null)
            {
                string drawnText = CreateTextToDraw(style, out List<(int Index, int Add)>? textLengthChanges);
                if (textLengthChanges != null)
                {
                    Vector2 localPos = TextSpaceToLocalSpace(keyInputString!.InputString!.Cursor, style, drawnText, textLengthChanges);
                    localPos.Y += style.LineSpacing * moveCursorVertically.Value;
                    keyInputString!.InputString!.Cursor = DrawSpaceToTextSpace(localPos, style);
                }

                moveCursorVertically = null;
            }

            if (style.FontColor.IsNotTransparent())
            {
                if (drawFocused)
                {
                    if (cursorChanged)
                    {
                        string drawnText = CreateTextToDraw(style, out List<(int Index, int Add)>? textLengthChanges);
                        if (textLengthChanges != null)
                        {
                            float lineSpacing = style.LineSpacing;

                            cursorChanged = false;
                            Vector2 cursorDrawPos = TextSpaceToLocalSpace(keyInputString!.InputString!.Cursor, style, drawnText, textLengthChanges);
                            cursorDrawPos = LocalToGlobalSpace(style, cursorDrawPos, drawnText);

                            cursorDraw = new Rect(cursorDrawPos, new Vector2(2, lineSpacing));

                            if (keyInputString!.InputString!.SelectStart != null)
                            {
                                Vector2 selectDrawPos = TextSpaceToLocalSpace(keyInputString!.InputString!.SelectStart.Value, style, drawnText, textLengthChanges);
                                selectDrawPos = LocalToGlobalSpace(style, selectDrawPos, drawnText);

                                Vector2 selectPosOnCursorLine = new Vector2(selectDrawPos.X, cursorDrawPos.Y);
                                if (selectDrawPos.Y < cursorDrawPos.Y)
                                {
                                    selectPosOnCursorLine.X = AbsoluteRect.Left;
                                }
                                else if (selectDrawPos.Y > cursorDrawPos.Y)
                                {
                                    selectPosOnCursorLine.X = AbsoluteRect.Right;
                                }

                                selectDraw = new()
                            {
                                // cursor line
                                new Rect(cursorDrawPos, selectPosOnCursorLine - cursorDrawPos + new Vector2(0f, lineSpacing)),
                            };

                                if (selectDrawPos.Y != cursorDrawPos.Y)
                                {
                                    // add select start line
                                    Vector2 selectPosOnSelectLine = selectDrawPos;
                                    if (selectDrawPos.Y < cursorDrawPos.Y)
                                    {
                                        selectPosOnSelectLine.X = AbsoluteRect.Right;
                                    }
                                    else
                                    {
                                        selectPosOnSelectLine.X = AbsoluteRect.Left;
                                    }
                                    selectDraw.Add(new Rect(selectDrawPos, selectPosOnSelectLine - selectDrawPos + new Vector2(0f, lineSpacing)));
                                }

                                // add lines in between
                                if (MathF.Abs(selectDrawPos.Y - cursorDrawPos.Y) > lineSpacing * 1.5f)
                                {
                                    float top, bottom;
                                    if (selectDrawPos.Y < cursorDrawPos.Y)
                                    {
                                        top = selectDrawPos.Y + lineSpacing;
                                        bottom = cursorDrawPos.Y;
                                    }
                                    else
                                    {
                                        top = cursorDrawPos.Y + lineSpacing;
                                        bottom = selectDrawPos.Y;
                                    }
                                    selectDraw.Add(new Rect(AbsoluteRect.Left, top, AbsoluteRect.Width, bottom - top));
                                }
                            }
                            else
                            {
                                selectDraw = null;
                            }
                        }
                    }

                    if (cursorDraw != null)
                    {
                        if ((updateSpeed.TotalMSF() - lastMSCursorOrSelectionChanged) % 1000 < 500)
                        {
                            cursorDraw.Draw(spriteBatch, style.FontColor.Value);
                        }
                    }
                    if (selectDraw != null)
                    {
                        for (int i = 0; i < selectDraw.Count; i++)
                        {
                            selectDraw[i].Draw(spriteBatch, style.FontColor.Value * 0.5f);
                        }
                    }
                }
            }

            // TODO: this is bad scrolling code, improve it. It also only works for Anchor.Y == 0 (which should be most text inputs)
            Vector2 textSize = style.MeasureString(TextToDraw);
            Vector2 restoreAnchor = Anchor;
            try
            {
                if (textSize.Y > AbsoluteRect.Height)
                {
                    Anchor = new Vector2(Anchor.X, 1f - Anchor.Y);
                }

                base.DrawSelf(spriteBatch, style);

            }
            finally
            {
                Anchor = restoreAnchor;
            }
        }

        private Vector2 LocalToGlobalSpace(StyleRoot style, Vector2 relativePos, string drawnText)
        {
            if (Anchor == Vector2.Zero)
            {
                relativePos += AbsoluteRect.Pos;
            }
            else
            {
                Vector2 stringSize = MeasureString(style, drawnText);
                stringSize.Y = MathF.Max(stringSize.Y, style.Font.Value.LineSpacing);
                relativePos += AbsoluteRect.GetAnchor(Anchor).Rectangle(stringSize).Pos;
            }

            // TODO: this is bad scrolling code, improve it.
            Vector2 textSize = style.MeasureString(TextToDraw);
            if (textSize.Y > AbsoluteRect.Height)
            {
                relativePos.Y -= textSize.Y - AbsoluteRect.Height;
            }

            return relativePos;
        }

        private Vector2 GlobalToLocalSpace(StyleRoot style, Vector2 absolutePos)
        {
            // TODO: this is bad scrolling code, improve it.
            Vector2 textSize = style.MeasureString(TextToDraw);
            if (textSize.Y > AbsoluteRect.Height)
            {
                absolutePos.Y += textSize.Y - AbsoluteRect.Height;
            }

            if (Anchor == Vector2.Zero)
            {
                absolutePos -= AbsoluteRect.Pos;
            }
            else
            {
                string drawnText = CreateTextToDraw(style, out _);
                absolutePos -= AbsoluteRect.GetAnchor(Anchor).Rectangle(MeasureString(style, drawnText)).Pos;
            }

            return absolutePos;
        }

        private Vector2 TextSpaceToLocalSpace(int cursorPosition, StyleRoot style, string drawnText, List<(int Index, int Add)> textLengthChanges)
        {
            if (cursorPosition == 0)
            {
                return Vector2.Zero;
            }

            // update cursorPosition to drawn Cursor position
            for (int i = 0; i < textLengthChanges.Count; i++)
            {
                int index = textLengthChanges[i].Index;
                int amount = textLengthChanges[i].Add;
                if (amount > 0)
                {
                    if (cursorPosition >= index)
                    {
                        cursorPosition += amount;
                    }
                }
                else
                {
                    if (cursorPosition > index)
                    {
                        int cursorDistanceToIndex = cursorPosition - index;
                        int removeAmountForCursor = Math.Min(-amount, cursorDistanceToIndex);
                        cursorPosition -= removeAmountForCursor;
                    }
                }
            }


            string drawnTextUntilCursor = drawnText.Remove(cursorPosition);

            Vector2 textPartSize = style.MeasureString(drawnTextUntilCursor);
            int lastNewLineChar = drawnTextUntilCursor.LastIndexOf('\n');
            float lastLineOfPartWidth;
            if (lastNewLineChar == -1)
            {
                lastLineOfPartWidth = textPartSize.X;
            }
            else
            {
                string lastLine = drawnTextUntilCursor.Substring(lastNewLineChar + 1);
                lastLineOfPartWidth = style.MeasureString(lastLine).X;
            }

            return new Vector2(lastLineOfPartWidth, textPartSize.Y - style.LineSpacing);
        }

        private int DrawSpaceToTextSpace(Vector2 localPos, StyleRoot style)
        {
            Vector2? lastCursorPos = null;

            string drawnText = CreateTextToDraw(style, out List<(int Index, int Add)>? textLengthChanges);

            if (textLengthChanges == null)
            {
                return 0;
            }

            for (int cursor = 0; cursor <= Text.Length; cursor++)
            {
                Vector2 cursorPos = TextSpaceToLocalSpace(cursor, style, drawnText, textLengthChanges);
                if (cursorPos.Y > localPos.Y) // case where end of a line is clicked
                {
                    return cursor;
                }
                else if (cursorPos.Y + style.LineSpacing > localPos.Y && cursorPos.X >= localPos.X)
                {
                    if (lastCursorPos != null 
                        && Vector2.DistanceSquared(localPos, lastCursorPos.Value)
                        < Vector2.DistanceSquared(localPos, cursorPos))
                    {
                        return cursor - 1;
                    }
                    else
                    {
                        return cursor;
                    }
                }
                lastCursorPos = cursorPos;
            }

            return Text.Length;
        }

        public void Focus()
        {
            doFocus = true;
        }

        public void FocusAndSelectAll()
        {
            doFocus = true;
            doSelectAllOnClick = true;
        }
    }
}
