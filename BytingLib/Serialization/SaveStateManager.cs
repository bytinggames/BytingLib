using System.Text.Json;

namespace BytingLib
{
    public class SaveStateManager
    {
        public static T LoadOrCreate<T>(uint id)
        {
            string filePath = GetFilePath(id);
            if (!File.Exists(filePath))
                return Activator.CreateInstance<T>();
            string json = File.ReadAllText(filePath);
            T? save = JsonSerializer.Deserialize<T>(json);
            if (save == null)
                throw new BytingException("Couldn't load save file");
            return save;
        }

        public static void Save<T>(T save, uint id) where T : notnull
        {
            string json = JsonSerializer.Serialize(save, save.GetType());
            string filePath = GetFilePath(id);
            File.WriteAllText(filePath, json);
        }

        private static string GetFilePath(uint id)
        {
            return Path.Combine(DefaultPaths.SaveStateDir, "save" + id + ".json");
        }
    }
}
