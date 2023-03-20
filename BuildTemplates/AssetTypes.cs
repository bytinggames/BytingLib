
namespace BuildTemplates
{
    internal static class AssetTypes
    {
        public static IReadOnlyDictionary<string, (string VarName, string[] Extensions)> Extensions { get; } = new Dictionary<string, (string, string[])>()
        {
            // { "<Class Name>", ("<Extension for Variable Name in ContentLoader>", new[] { "<file extension 1>", "<file extension 2>" }) },
            { "Texture2D", ("Tex", new[] { "png", "jpeg", "jpg" }) },
            { "SpriteFont", ("Font", new[] { "spritefont" } )},
            { "SoundEffect", ("Sfx", new[] { "wav", "mp3", "ogg" } )},
            { "Effect", ("Fx", new[] { "fx" } )},
            { "string", ("Txt", new[] { "json", "ini", "config", "txt" } )},
            { "Model", ("Model", new[] { "fbx" }) },
            { "MyModel", ("Model", new[] { "myfbx" }) },
            { "ModelGL", ("ModelGL", new[] { "gltf" }) },
            { "CollisionMesh", ("Mesh", new[] { "colmesh" }) },
            { "CollisionMeshGrid", ("Grid", new[] { "colgrid" }) },
            { "byte[]", ("Bytes", new[] { "bin" }) },
            { "Animation", ("Ani", new[] { "ani" }) }

            // when adding new asset types, also update:
            // maybe update ContentTemplate.cs File() constructor
            // maybe update ContentTemplate.cs PrintMGCB()
            // DirectorySupervisor.cs FileStamp.AssetName
            // HotReloadContent.cs GetFiles() Get("..."); + maybe dependencies
            // ContentManagerRaw.cs Load<T>()
            // ExtensionToAssetType.cs Convert()
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
