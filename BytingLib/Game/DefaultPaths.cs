using System.Reflection;

namespace BytingLib
{
    public static class DefaultPaths
    {
        public static string GameAppDataDir { get; }
        public static string InputRecordingsDir { get; }
        public static string SaveStateDir { get; }

        static DefaultPaths()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string? gameName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (gameName == null)
                throw new BytingException("couldn't read game name");
            GameAppDataDir = Path.Combine(appDataDir, gameName);
            InputRecordingsDir = Path.Combine(GameAppDataDir, "InputRecordings");
            SaveStateDir = Path.Combine(DefaultPaths.GameAppDataDir, "saves");
            Directory.CreateDirectory(SaveStateDir);
        }
    }
}
