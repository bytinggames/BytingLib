namespace BytingLib
{
    public class FileDrop : IDisposable
    {
        private readonly GameWindow window;
        private readonly Action<string[]> droppedFiles;

        public FileDrop(GameWindow window, Action<string[]> droppedFiles)
        {
            this.window = window;
            this.droppedFiles = droppedFiles;
            window.FileDrop += Window_FileDrop;
        }

        public void Dispose()
        {
            window.FileDrop -= Window_FileDrop;
        }

        private void Window_FileDrop(object? sender, FileDropEventArgs e)
        {
            if (e.Files == null
                || e.Files.Length == 0)
            {
                return;
            }

            droppedFiles.Invoke(e.Files);
        }
    }
}
