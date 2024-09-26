using Microsoft.Extensions.Configuration;

namespace BytingLib
{
    public interface ISettingsManager
    {
        void UpdateCheckChanges();
    }

    public class SettingsManager<_Settings> : ISettingsManager where _Settings : new()
    {
        public _Settings Settings { get; private set; }

        private readonly DirectorySupervisor dirSupervisor;
        private readonly IConfigurationRoot configRoot;
        private readonly DefaultPaths paths;
        private readonly string[] programArgs;

        public SettingsManager(DefaultPaths paths, string[]? programArgs)
        {
            this.paths = paths;
            this.programArgs = programArgs ?? [];
            configRoot = new ConfigurationBuilder()
                .AddYamlFile(paths.SettingsFile, true)
#if DEBUG
                .AddYamlFile(paths.SettingsDebugFile, true)
#endif
                .Build();

            Settings = CreateSettings();

            dirSupervisor = new DirectorySupervisor(paths.GameAppDataDir, GetSettingsFiles, false);
        }

        private _Settings CreateSettings() => configRoot.Get<_Settings>() ?? new();

        private void ReloadConfig()
        {
            configRoot.Reload();
            Settings = CreateSettings();
        }

        public void UpdateCheckChanges()
        {
            // only reload config in debug mode
#if DEBUG
            var changes = dirSupervisor.GetChanges();
            if (changes.Any())
            {
                ReloadConfig();
            }
#endif
        }

        private string[] GetSettingsFiles()
        {
            List<string> files = new();
            if (File.Exists(paths.SettingsFile))
            {
                files.Add(paths.SettingsFile);
            }
#if DEBUG
            if (File.Exists(paths.SettingsDebugFile))
            {
                files.Add(paths.SettingsDebugFile);
            }
#endif
            return files.ToArray();
        }

        public bool ShouldExampleYamlFileBeCreated()
        {
#if DEBUG
            return true;
#else
            return programArgs.Contains("example_settings");
#endif
        }

        public void CreateExampleYamlFileIfNotExisting(string? cSharpFile)
        {
            // create settings example yaml
            if (!File.Exists(paths.SettingsExampleFile))
            {
                var settings = Activator.CreateInstance<_Settings>();

                if (settings != null)
                {
                    if (cSharpFile == null)
                    {
                        // generate yaml example with comments
                        var serializer = new YamlDotNet.Serialization.Serializer();
                        string yaml = serializer.Serialize(settings);
                        File.WriteAllText(paths.SettingsExampleFile, yaml);
                    }
                    else
                    {
                        // generate yaml example without comments
                        string yaml = SettingsExampleGenerator.GenerateYaml(settings,
                            cSharpFile);

                        File.WriteAllText(paths.SettingsExampleFile, yaml);
                    }
                }
            }
        }
    }
}
