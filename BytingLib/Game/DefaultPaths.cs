using System.Reflection;

namespace BytingLib
{
    public class DefaultPaths
    {
        public string GameAppDataDir { get; }
        public string InputRecordingsDir { get; }
        public string SaveStateDir { get; }
        public string ScreenshotsDir { get; }
        public string RandomScreenshotsDir { get; }
        public string SettingsFile { get; }
        public string SettingsDebugFile { get; }
        public string SettingsExampleFile { get; }
        public string CrashLogFile { get; }

        public DefaultPaths(bool appdataNextToExe = false)
        {
            string? gameName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (gameName == null)
            {
                throw new BytingException("couldn't read game name");
            }

#if LINUX
            Environment.CurrentDirectory = AppContext.BaseDirectory; // this ensures that the current directory is actually the one that the exe is in.
#endif
#if OSX
            // if on OSX, set current directory to Resources path which is above Content directory. But NOT the directory that contains the application
            Environment.CurrentDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources"));
            // if on OSX, store appdata stuff in Resources directory inside th .app bundle
            string appDataDir = Path.Combine(Environment.CurrentDirectory, "AppData");
            Directory.CreateDirectory(appDataDir);
            GameAppDataDir = Path.Combine(appDataDir, gameName);
#else
            if (appdataNextToExe)
            {
                GameAppDataDir = Path.Combine(AppContext.BaseDirectory, "UserData");
            }
            else
            {
                string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                GameAppDataDir = Path.Combine(appDataDir, gameName);
            }
#endif
            InputRecordingsDir = Path.Combine(GameAppDataDir, "input-recordings");
            SaveStateDir = Path.Combine(GameAppDataDir, "saves");
            Directory.CreateDirectory(SaveStateDir);
            ScreenshotsDir = Path.Combine(GameAppDataDir, "screenshots");
            RandomScreenshotsDir = Path.Combine(GameAppDataDir, "screenshots-random");
            SettingsFile = Path.Combine(GameAppDataDir, "settings.yaml");
            SettingsDebugFile = Path.Combine(GameAppDataDir, "settings.debug.yaml");
            SettingsExampleFile = Path.Combine(GameAppDataDir, "settings.example.yaml");
            CrashLogFile = Path.Combine(GameAppDataDir, "crash.log");
        }

        public static string GetCurrentDateTimeFilename() => DateTimeToFilename(DateTime.Now);

        public static string DateToFilename(DateTime dateTime)
        {
            return dateTime.ToString("yyyy.MM.dd");
        }

        public static string TimeToFilename(DateTime dateTime)
        {
            return dateTime.ToString("HH.mm.ss_fff");
        }

        public static string DateTimeToFilename(DateTime dateTime)
        {
            return dateTime.ToString("yyyy.MM.dd_HH.mm.ss_fff");
        }

        public string GetNewScreenshotWithoutEnding()
        {
            Directory.CreateDirectory(ScreenshotsDir);
            return Path.Combine(ScreenshotsDir, GetCurrentDateTimeFilename());
        }
        internal string GetNewRandomScreenshotWithoutEnding()
        {
            Directory.CreateDirectory(RandomScreenshotsDir);
            return Path.Combine(RandomScreenshotsDir, GetCurrentDateTimeFilename());
        }

        public string GetNewScreenshotPng() => GetNewScreenshotWithoutEnding() + ".png";
        internal string GetNewRandomScreenshotPng() => GetNewRandomScreenshotWithoutEnding() + ".png";
    }
}
