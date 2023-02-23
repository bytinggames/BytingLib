using System.Text.Json;

namespace BytingLib.Serialization
{
    public class SaveStateManager
    {
        private readonly DefaultPaths paths;

        public SaveStateManager(DefaultPaths paths)
        {
            this.paths = paths;
        }

        public T LoadOrCreate<T>(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
                return Activator.CreateInstance<T>();
            string json = File.ReadAllText(filePath);
            T? save = JsonSerializer.Deserialize<T>(json);
            if (save == null)
                throw new BytingException("Couldn't load save file");
            return save;
        }

        public void Save<T>(T save, string fileName) where T : notnull
        {
            string json = JsonSerializer.Serialize(save, save.GetType());
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, json);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(paths.SaveStateDir, fileName + ".json");
        }
    }
}
