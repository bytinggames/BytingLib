using System.Diagnostics;
using System.Text.Json;

namespace BytingLib.Serialization
{
    public class SaveStateManager
    {
        private readonly string saveStateDir;
        public bool ThrowExceptionWhenLoadingTooNewVersion { get; set; }
        public event Action<Exception, string>? OnException;

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            Converters =
            {
                new ValueEventStringJsonConverter(),
                new ValueEventIntJsonConverter(),
                new ValueEventFloatJsonConverter(),
                new ValueEventFloatNullableJsonConverter(),
                new ValueEventBoolJsonConverter(),
                new ValueEventColorJsonConverter(),
                new DateTimeMSJsonConverter(),
            }
        };

        public SaveStateManager(string saveStateDir, bool throwExceptionWhenLoadingTooNewVersion)
        {
            this.saveStateDir = saveStateDir;
            this.ThrowExceptionWhenLoadingTooNewVersion = throwExceptionWhenLoadingTooNewVersion;
        }

        public T LoadOrCreate<T>(string saveStateName, out bool createdNewSaveState)
        {
            string filePath = GetFilePath(saveStateName);
            if (!File.Exists(filePath))
            {
                createdNewSaveState = true;
                return Activator.CreateInstance<T>();
            }

            string json = File.ReadAllText(filePath);
            T? save = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (save == null)
            {
                throw new BytingException("Couldn't load save file");
            }

            createdNewSaveState = false;
            return save;
        }

        public void Save<T>(T save, string fileName) where T : notnull
        {
            string json = JsonSerializer.Serialize(save, save.GetType(), JsonOptions);
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, json);
        }

        public T? Load<T>(string saveStateName, Migrator<T> migrator)
        {
            string filePath = GetFilePath(saveStateName);
            if (!File.Exists(filePath))
            {
                return default;
            }

            string json = File.ReadAllText(filePath);
            T? save = default;
            bool threwException = false;
            uint? tooNewVersion = null;

            try
            {
                save = migrator.Deserialize(json, ThrowExceptionWhenLoadingTooNewVersion, out tooNewVersion);
            }
            catch (Exception e)
            {
                threwException = true;
                OnException?.Invoke(e, "exception when trying to deserialize json: " + json);
            }
            if (save == null)
            {
                if (!threwException)
                {
                    OnException?.Invoke(new BytingException("failed to deserialize json: " + json), "");
                }
                return default;
            }

            if (tooNewVersion != null)
            {
                int i = 1;
                while (true)
                {
                    string filename = Path.Combine(Path.GetDirectoryName(filePath) ?? "", $"{Path.GetFileNameWithoutExtension(filePath)}_backup_v{tooNewVersion.Value}_{i}.json");
                    i++;
                    if (!File.Exists(filename))
                    {
                        try
                        {
                            File.Copy(filePath, filename, true);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("exception when trying to backup savestate: " + e.ToString());
                        }
                        break;
                    }
                }
            }

            return save;
        }

        public T LoadOrCreate<T>(string saveStateName, out bool createdNewSaveState, Migrator<T> migrator)
        {
            T? saveState = Load(saveStateName, migrator);
            if (saveState == null)
            {
                createdNewSaveState = true;
                return Activator.CreateInstance<T>();
            }

            createdNewSaveState = false;
            return saveState;
        }

        public void Save<T>(T save, string fileName, Migrator<T> migrator)
        {
            string json = migrator.Serialize(save);
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, json);
        }

        public string GetFilePath(string saveStateName)
        {
            return Path.Combine(saveStateDir, saveStateName + ".json");
        }
    }
}
