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
        public string CrashLogFile { get; }

        public DefaultPaths()
        {
#if OSX
            // if on OSX, set current directory to Resources path which is above Content directory. But NOT the directory that contains the application
            Environment.CurrentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources"));
            // if on OSX, store appdata stuff in Resources directory inside th .app bundle
            string appDataDir = Path.Combine(Environment.CurrentDirectory, "AppData");
            Directory.CreateDirectory(appDataDir);
#else
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#endif
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
            CrashLogFile = Path.Combine(GameAppDataDir, "crash.log");
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
