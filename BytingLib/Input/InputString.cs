using System.Text;

namespace BytingLib
{
    public class InputString
    {
        StringBuilder text;

        public int Cursor { get; set; }
        public int? SelectStart { get; set; }

        public string Text
        {
            get => text.ToString();
            set
            {
                text = new StringBuilder(value);
                Cursor = text.Length;
                SelectStart = null;
            }
        }

        public event Action<InputString>? OnTextChange;
        public event Action<InputString>? OnCursorMoveOrSelectChanged;

        public InputString()
        {
            text = new StringBuilder();
        }

        //public void Update()
        //{
        //    foreach (var key in keys.GetPressedKeys())
        //    {
        //        Execute(key);
        //    }
        //}

        public void Delete(int amountToRight)
        {
            if (RemoveSelectIfSelected())
            {
                OnTextChange?.Invoke(this);
                return;
            }

            int deleteTo = Cursor + amountToRight;
            if (deleteTo < 0)
                deleteTo = 0;
            else if (deleteTo > text.Length)
                deleteTo = text.Length;

            if (deleteTo == Cursor)
                return;

            int deleteCount;
            if (deleteTo < Cursor)
            {
                deleteCount = Cursor - deleteTo;
                Cursor = deleteTo;
            }
            else
            {
                deleteCount = deleteTo - Cursor;
            }

            text.Remove(Cursor, deleteCount);
            OnTextChange?.Invoke(this);
        }
        public void Insert(char c)
        {
            RemoveSelectIfSelected();

            text.Insert(Cursor, c);
            Cursor++;
            OnTextChange?.Invoke(this);
        }

        /// <summary>
        /// doesn't call OnTextChange?.Invoke()
        /// </summary>
        private bool RemoveSelectIfSelected()
        {
            if (!CheckIfAnyIsSelected())
                return false;

            int min = Math.Min(SelectStart.Value, Cursor);
            int max = Math.Max(SelectStart.Value, Cursor);
            text.Remove(min, max - min);
            Cursor = min;
            StopSelectInternal();
            return true;
        }

        public void Insert(string str)
        {
            RemoveSelectIfSelected();

            text.Insert(Cursor, str);
            Cursor += str.Length;
            OnTextChange?.Invoke(this);
        }

        public bool MoveCursorHorizontally(int toTheRight)
        {
            if (toTheRight == 0)
                return false;
            int newCursor = Cursor + toTheRight;
            newCursor = Math.Clamp(newCursor, 0, text.Length);
            if (newCursor == Cursor)
                return false;

            Cursor = newCursor;
            OnCursorMoveOrSelectChanged?.Invoke(this);

            return true;
        }

        public void MoveOverWordLeft()
        {
            if (!MoveCursorHorizontally(-1))
                return;

            if (!IsCharFromWord(text[Cursor]))
                return;

            while (Cursor > 0 && IsCharFromWord(text[Cursor - 1]))
            {
                Cursor--;
            }
            OnCursorMoveOrSelectChanged?.Invoke(this);
        }

        private bool IsCharFromWord(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        public void MoveOverWordRight()
        {
            if (!MoveCursorHorizontally(1))
                return;

            if (!IsCharFromWord(text[Cursor - 1]))
                return;

            while (Cursor < text.Length && IsCharFromWord(text[Cursor]))
            {
                Cursor++;
            }
            OnCursorMoveOrSelectChanged?.Invoke(this);
        }

        public void Cut()
        {
            if (!CheckIfAnyIsSelected())
                return;

            TextCopy.ClipboardService.SetText(GetSelected());

            Delete(-1);
        }

        public void Copy()
        {
            if (!CheckIfAnyIsSelected())
                return;

            TextCopy.ClipboardService.SetText(GetSelected());
        }

        public void Paste()
        {
            Insert(TextCopy.ClipboardService.GetText());
        }

        private string GetSelected()
        {
            if (!CheckIfAnyIsSelected())
                return "";

            int min = Math.Min(SelectStart!.Value, Cursor);
            int max = Math.Max(SelectStart.Value, Cursor);
            return text.ToString().Substring(min, max - min);
        }


        public bool MoveCursorVertically(int downwards)
        {
            return false;
        }

        public void EnsureSelect()
        {
            if (SelectStart == null)
                SelectStart = Cursor;
        }

        public bool StopSelectOnLeftSide()
        {
            if (!CheckIfAnyIsSelected())
                return false;

            if (SelectStart!.Value < Cursor)
                Cursor = SelectStart.Value;

            StopSelectInternal();
            OnCursorMoveOrSelectChanged?.Invoke(this);
            return true;
        }

        public bool StopSelectOnRight()
        {
            if (!CheckIfAnyIsSelected())
                return false;

            if (SelectStart!.Value > Cursor)
                Cursor = SelectStart.Value;

            StopSelectInternal();
            OnCursorMoveOrSelectChanged?.Invoke(this);
            return true;
        }

        private bool CheckIfAnyIsSelected()
        {
            if (SelectStart == null)
                return false;

            if (SelectStart.Value == Cursor)
            {
                StopSelectInternal();
                return false;
            }
            return true;
        }

        private void StopSelectInternal()
        {
            SelectStart = null;
        }

        public void DeleteWordLeft()
        {
            if (RemoveSelectIfSelected())
            {
                OnTextChange?.Invoke(this);
                return;
            }

            int start = Cursor;
            MoveOverWordLeft();
            Delete(start - Cursor);
        }

        public void DeleteWordRight()
        {
            if (RemoveSelectIfSelected())
            {
                OnTextChange?.Invoke(this);
                return;
            }

            int start = Cursor;
            MoveOverWordRight();
            Delete(start - Cursor);
        }
    }
}
