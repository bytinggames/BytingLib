
namespace BuildTemplates
{
    internal static class AssetTypes
    {
        public static IReadOnlyDictionary<string, (string VarName, string[] Extensions)> Extensions { get; } = new Dictionary<string, (string, string[])>()
        {
            { "Texture2D", ("Tex", new[] { "png", "jpeg", "jpg" }) },
            { "SpriteFont", ("Font", new[] { "spritefont" } )},
            { "SoundEffect", ("Sfx", new[] { "wav", "mp3", "ogg" } )},
            { "Effect", ("Fx", new[] { "fx" } )},
            { "string", ("Txt", new[] { "json", "ini", "config" } )},
            { "MyModel", ("Model", new[] { "fbx" }) },
        };

        public static string? Convert(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return null;
            if (extension[0] == '.')
                extension = extension[1..];

            foreach (var ex in Extensions)
            {
                if (ex.Value.Extensions.Any(f => f == extension))
                    return ex.Key;
            }
            throw new NotImplementedException();
        }
    }
}
