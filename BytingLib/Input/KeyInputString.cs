using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class KeyInputString : IDisposable
    {
        private readonly GameWindow window;
        public InputString? InputString { get; set; }

        private bool control, shift;

        public KeyInputString(GameWindow window)
        {
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            window.TextInput += ReceiveTextInput;
            this.window = window;
        }

        private void Window_KeyUp(object? sender, InputKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.LeftControl:
                case Keys.RightControl:
                    control = false;
                    break;
                case Keys.LeftShift:
                case Keys.RightShift:
                    shift = false;
                    break;
            }
        }

        private void Window_KeyDown(object? sender, InputKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.LeftControl:
                case Keys.RightControl:
                    control = true;
                    break;
                case Keys.LeftShift:
                case Keys.RightShift:
                    shift = true;
                    break;
            }

            if (InputString == null)
                return;

            switch (e.Key)
            {
                case Keys.Left:
                    if (shift)
                        InputString.EnsureSelect();
                    else if (InputString.StopSelectOnLeftSide() && !control)
                        break;
                    if (control)
                        InputString.MoveOverWordLeft();
                    else
                        InputString.MoveCursorHorizontally(-1);
                    break;

                case Keys.Right:
                    if (shift)
                        InputString.EnsureSelect();
                    else if (InputString.StopSelectOnRight() && !control)
                        break;
                    if (control)
                        InputString.MoveOverWordRight();
                    else
                        InputString.MoveCursorHorizontally(1);
                    break;

                case Keys.Up:
                    InputString.MoveCursorVertically(-1);
                    break;

                case Keys.Down:
                    InputString.MoveCursorVertically(1);
                    break;

                case Keys.C:
                    if (control)
                        InputString.Copy();
                    break;
                case Keys.V:
                    if (control)
                        InputString.Paste();
                    break;
                case Keys.X:
                    if (control)
                        InputString.Cut();
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
            {
                InputString.Insert(e.Character);
            }
            else
            {
                switch (e.Key)
                {
                    case Keys.Back:
                        if (control)
                            InputString.DeleteWordLeft();
                        else
                            InputString.Delete(-1);
                        break;
                    case Keys.Delete:
                        if (control)
                            InputString.DeleteWordRight();
                        else
                            InputString.Delete(1);
                        break;
                }
            }
        }
    }
}
