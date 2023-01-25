using System.Reflection;

namespace BytingLib
{
    public static class DefaultPaths
    {
        public readonly static string GameAppDataDir;
        public readonly static string InputRecordingsDir;

        static DefaultPaths()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string? gameName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (gameName == null)
                throw new BytingException("couldn't read game name");
            GameAppDataDir = Path.Combine(appDataDir, gameName);
            InputRecordingsDir = Path.Combine(GameAppDataDir, "InputRecordings");
        }
    }
}
