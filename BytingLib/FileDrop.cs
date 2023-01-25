namespace BytingLib
{
    public class FileDrop : IDisposable
    {
        private readonly GameWindow window;
        private readonly Action<string> droppedFile;

        public FileDrop(GameWindow window, Action<string> droppedFile)
        {
            this.window = window;
            this.droppedFile = droppedFile;
            window.FileDrop += Window_FileDrop;
        }

        public void Dispose()
        {
            window.FileDrop -= Window_FileDrop;
        }

        private void Window_FileDrop(object? sender, FileDropEventArgs e)
        {
            if (e.Files?.Length > 0)
                droppedFile.Invoke(e.Files[0]);
        }
    }
}
