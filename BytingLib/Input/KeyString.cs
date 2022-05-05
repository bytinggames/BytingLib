
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace BytingLib
{
    //interface IKeyChar
    //{
    //    void Execute(IInputString textEditor);
    //}

    //interface IInputString
    //{
    //    string Text { get; set; }
    //    int Cursor { get; set; }
    //    int Selection { get; set; }
    //}

    public class KeyString : IDisposable
    {
        private readonly GameWindow window;
        public IInputString? InputString { get; set; }

        public KeyString(GameWindow window)
        {
            window.KeyDown += Window_KeyDown;
            window.TextInput += ReceiveTextInput;
            this.window = window;
        }

        private void Window_KeyDown(object? sender, InputKeyEventArgs e)
        {
            if (InputString == null)
                return;

            switch (e.Key)
            {
                case Keys.Left:
                    InputString.MoveCursorHorizontally(-1);
                    break;
                case Keys.Right:
                    InputString.MoveCursorHorizontally(1);
                    break;
                case Keys.Up:
                    InputString.MoveCursorVertically(-1);
                    break;
                case Keys.Down:
                    InputString.MoveCursorVertically(1);
                    break;
            }
        }

        public void Dispose()
        {
            window.TextInput -= ReceiveTextInput;
        }

        void ReceiveTextInput(object? sender, TextInputEventArgs e)
        {
            if (InputString == null)
                return;

            if (!char.IsControl(e.Character))
                InputString.Insert(e.Character);
            else
            {
                switch (e.Key)
                {
                    case Keys.Back:
                        InputString.Delete(-1);
                        break;
                    case Keys.Delete:
                        InputString.Delete(1);
                        break;
                }
            }
        }
    }

    public interface IInputString
    {
        void Delete(int amountToRight);
        void Insert(char c);
        void MoveCursorHorizontally(int toTheRight);
        void MoveCursorVertically(int downwards);
    }

    public class InputString : IInputString
    {
        StringBuilder text;

        public int Cursor { get; set; }
        public int Selection { get; set; }

        public string Text
        {
            get => text.ToString();
            set
            {
                text = new StringBuilder(value);
                Cursor = text.Length;
                Selection = 0;
            }
        }

        public event Action<InputString>? OnTextChange;
        public event Action<InputString>? OnCursorMove;

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
            text.Insert(Cursor, c);
            Cursor++;
            OnTextChange?.Invoke(this);
        }
        public void Insert(string str)
        {
            text.Insert(Cursor, str);
            Cursor += str.Length;
            OnTextChange?.Invoke(this);
        }

        public void MoveCursorHorizontally(int toTheRight)
        {
            if (toTheRight == 0)
                return;
            int newCursor = Cursor + toTheRight;
            newCursor = Math.Clamp(newCursor, 0, text.Length);
            if (newCursor == Cursor)
                return;

            Cursor = newCursor;
            OnCursorMove?.Invoke(this);
        }


        private bool IsAltGr()
        {
            throw new NotImplementedException();
        }

        private void ToggleInsertMode()
        {
            throw new NotImplementedException();
        }

        private void CursorDown()
        {
            throw new NotImplementedException();
        }

        private void CursorUp()
        {
            throw new NotImplementedException();
        }

        private void CursorToEnd()
        {
            throw new NotImplementedException();
        }

        private void DeleteWord()
        {
            throw new NotImplementedException();
        }

        public void MoveCursorVertically(int downwards)
        {
        }
    }
}
