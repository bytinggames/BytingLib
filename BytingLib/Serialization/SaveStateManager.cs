using System.Text.Json;

namespace BytingLib.Serialization
{
    public class SaveStateManager
    {
        private readonly DefaultPaths paths;

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            Converters =
            {
                new ValueEventStringJsonConverter(),
                new ValueEventIntJsonConverter(),
                new ValueEventFloatJsonConverter(),
                new ValueEventBoolJsonConverter(),
                new DateTimeMSJsonConverter(),
            }
        };

        public SaveStateManager(DefaultPaths paths)
        {
            this.paths = paths;
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
            T? save = JsonSerializer.Deserialize<T>(json);
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
            T? save = migrator.Deserialize(json);
            if (save == null)
            {
                throw new BytingException("Couldn't load save file");
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
            return Path.Combine(paths.SaveStateDir, saveStateName + ".json");
        }
    }
}
