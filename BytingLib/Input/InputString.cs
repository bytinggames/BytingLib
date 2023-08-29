using System.Text;

namespace BytingLib
{
    public class InputString
    {
        StringBuilder text;

        private int _cursor;
        public int Cursor 
        { 
            get => _cursor;
            set
            {
                if (_cursor != value)
                {
                    _cursor = value;
                    OnCursorMoveOrSelectChanged?.Invoke(this);
                }
            }
        }
        private int? _selectStart;
        public int? SelectStart
        {
            get => _selectStart;
            set
            {
                if (_selectStart != value)
                {
                    _selectStart = value;
                    OnCursorMoveOrSelectChanged?.Invoke(this);
                }
            }
        }

        public string Text
        {
            get => text.ToString();
            set
            {
                text = new StringBuilder(value);
                _selectStart = null;
                _cursor = text.Length;
                OnCursorMoveOrSelectChanged?.Invoke(this);
            }
        }

        public event Action<InputString>? OnTextChange;
        public event Action<InputString>? OnCursorMoveOrSelectChanged;

        public Action<int>? MoveCursorVertically { get; set; }

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
            bool moveCursor = deleteTo < Cursor;
            if (moveCursor)
            {
                deleteCount = Cursor - deleteTo;
                _cursor = deleteTo;
            }
            else
            {
                deleteCount = deleteTo - Cursor;
            }

            text.Remove(Cursor, deleteCount);
            OnTextChange?.Invoke(this);
            if (moveCursor)
                OnCursorMoveOrSelectChanged?.Invoke(this);
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

            int min = Math.Min(SelectStart!.Value, Cursor);
            int max = Math.Max(SelectStart.Value, Cursor);
            text.Remove(min, max - min);
            StopSelectInternal();
            Cursor = min;
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

            return true;
        }

        public void MoveOverWordLeft()
        {
            if (!MoveCursorHorizontally(-1))
                return;

            if (!IsCharFromWord(text[Cursor]))
                return;

            while (_cursor > 0 && IsCharFromWord(text[_cursor - 1]))
            {
                _cursor--;
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

            while (_cursor < text.Length && IsCharFromWord(text[_cursor]))
            {
                _cursor++;
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
            string? text = TextCopy.ClipboardService.GetText();
            if (text == null)
                return;
            Insert(text);
        }

        private string GetSelected()
        {
            if (!CheckIfAnyIsSelected())
                return "";

            int min = Math.Min(SelectStart!.Value, Cursor);
            int max = Math.Max(SelectStart.Value, Cursor);
            return text.ToString().Substring(min, max - min);
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

            if (SelectStart!.Value < _cursor)
                _cursor = SelectStart.Value;

            StopSelectInternal();
            OnCursorMoveOrSelectChanged?.Invoke(this);
            return true;
        }

        public bool StopSelectOnRight()
        {
            if (!CheckIfAnyIsSelected())
                return false;

            if (SelectStart!.Value > _cursor)
                _cursor = SelectStart.Value;

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

        public void SelectAll()
        {
            _selectStart = 0;
            _cursor = text.Length;
            OnCursorMoveOrSelectChanged?.Invoke(this);
        }
    }
}
