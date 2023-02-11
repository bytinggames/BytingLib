using System.Reflection;

namespace BytingLib
{
    public class DefaultPaths
    {
        public string GameAppDataDir { get; }
        public string InputRecordingsDir { get; }
        public string SaveStateDir { get; }
        public string ScreenshotsDir { get; }
        public string SettingsFile { get; }
        public string SettingsDebugFile { get; }

        public DefaultPaths()
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
            SettingsFile = Path.Combine(GameAppDataDir, "settings.yaml");
            SettingsDebugFile = Path.Combine(GameAppDataDir, "settings.debug.yaml");
        }

        public static string GetCurrentDateTimeFilename()
        {
            return DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss_fff");
        }

        internal string GetNewScreenshotPng()
        {
            return Path.Combine(ScreenshotsDir, GetCurrentDateTimeFilename() + ".png");
        }
    }
}
