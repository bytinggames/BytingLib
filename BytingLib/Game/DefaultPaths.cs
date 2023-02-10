using System.Reflection;

namespace BytingLib
{
    public static class DefaultPaths
    {
        public static string GameAppDataDir { get; }
        public static string InputRecordingsDir { get; }
        public static string SaveStateDir { get; }
        public static string ScreenshotsDir { get; }

        static DefaultPaths()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string? gameName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (gameName == null)
                throw new BytingException("couldn't read game name");
            GameAppDataDir = Path.Combine(appDataDir, gameName);
            InputRecordingsDir = Path.Combine(GameAppDataDir, "input-recordings");
            SaveStateDir = Path.Combine(GameAppDataDir, "saves");
            Directory.CreateDirectory(SaveStateDir);
            ScreenshotsDir = Path.Combine(GameAppDataDir, "screenshots");
            Directory.CreateDirectory(ScreenshotsDir);
        }

        public static string GetCurrentDateTimeFilename()
        {
            return DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss_fff");
        }

        internal static string GetNewScreenshotPng()
        {
            return Path.Combine(ScreenshotsDir, GetCurrentDateTimeFilename() + ".png");
        }
    }
}
