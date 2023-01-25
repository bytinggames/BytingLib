namespace BytingLib
{
    public class FileSession : IDisposable
    {
        private readonly string sessionFile;
        private readonly string unsavedLevelFile;
        private readonly Action<string> save;
        private readonly Action<string> open;
        private readonly Action newFile;
        private readonly FileSessionData data;

        public event Action<string?>? OnCurrentFileChanged;
        public event Action<bool>? OnIsDirtyChanged;

        public FileSession(string sessionFile, string unsavedLevelFile, Action<string> save, Action<string> open, Action newFile)
        {
            this.sessionFile = sessionFile;
            this.unsavedLevelFile = unsavedLevelFile;
            this.save = save;
            this.open = open;
            this.newFile = newFile;
            Directory.CreateDirectory(Path.GetDirectoryName(sessionFile)!);
            Directory.CreateDirectory(Path.GetDirectoryName(unsavedLevelFile)!);

            if (File.Exists(sessionFile))
            {
                string json = File.ReadAllText(sessionFile);
                var d = System.Text.Json.JsonSerializer.Deserialize<FileSessionData>(json);
                if (d != null)
                    data = d;
                else
                    throw new BytingException("couldn't deserialize json file " + sessionFile);
            }
            else
            {
                data = new FileSessionData();
            }

            data.OnCurrentFileChanged += f => OnCurrentFileChanged?.Invoke(f);
            data.OnIsDirtyChanged += f => OnIsDirtyChanged?.Invoke(f);

            // load last session file
            if (data.IsDirty && File.Exists(unsavedLevelFile))
            {
                open(unsavedLevelFile);
            }
            else if (data.CurrentFile != null)
            {
                string file = data.CurrentFile;
                Discard(false);
                Open(file);
            }
        }

        public void SetUnsaved() => data.IsDirty = true;

        public bool IsUnsaved => data.IsDirty;

        public bool Save()
        {
            if (data.CurrentFile == null)
                return false;

            SaveAs(data.CurrentFile);
            return true;
        }

        public void SaveAs(string file)
        {
            data.CurrentFile = file;

            save(file);
            data.IsDirty = false;
            SaveFileSession();
        }

        public bool Open(string file)
        {
            if (data.IsDirty)
                return false;
            open(file);
            data.CurrentFile = file;
            data.IsDirty = false;
            SaveFileSession();
            return true;
        }

        public string? GetCurrentFile() => data.CurrentFile;

        public void Discard(bool saveSession = true)
        {
            data.CurrentFile = null;
            data.IsDirty = false;
            SaveFileSession();
        }

        public bool New()
        {
            if (data.IsDirty)
                return false;

            newFile();
            data.CurrentFile = null;
            SaveFileSession();
            return true;
        }

        public void Dispose()
        {
            if (data.IsDirty)
                save(unsavedLevelFile);

            SaveFileSession();
        }

        private void SaveFileSession()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(data);
            using (var stream = File.CreateText(sessionFile))
                stream.Write(json);
        }

        class FileSessionData
        {
            private string? _currentFile;
            private bool _isDirty;

            public string? CurrentFile
            {
                get => _currentFile;
                set
                {
                    if (_currentFile != value)
                    {
                        _currentFile = value;
                        OnCurrentFileChanged?.Invoke(_currentFile);
                    }
                }
            }
            public bool IsDirty
            {
                get => _isDirty;
                set
                {
                    if (_isDirty != value)
                    {
                        _isDirty = value;
                        OnIsDirtyChanged?.Invoke(_isDirty);
                    }
                }
            }

            public event Action<string?>? OnCurrentFileChanged;
            public event Action<bool>? OnIsDirtyChanged;
        }
    }
}
