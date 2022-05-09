
namespace BuildTemplates
{
    internal static class AssetTypes
    {
        public static IReadOnlyDictionary<string, string[]> Extensions { get; } = new Dictionary<string, string[]>()
        {
            { "Texture2D", new[] { "png", "jpeg", "jpg" } },
            { "SpriteFont", new[] { "spritefont" } },
            { "SoundEffect", new[] { "wav", "mp3", "ogg" } },
            { "Effect", new[] { "fx" } },
            { "string", new[] { "json", "ini", "config" } },
        };

        public static string? Convert(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return null;
            if (extension[0] == '.')
                extension = extension[1..];

            foreach (var ex in Extensions)
            {
                if (ex.Value.Any(f => f == extension))
                    return ex.Key;
            }
            throw new NotImplementedException();
        }
    }
}
