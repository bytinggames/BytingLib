
namespace BuildTemplates
{
    internal static class AssetTypes
    {
        internal static readonly Dictionary<string, string> ProcessorToDataType = new()
        {
            { "EffectProcessor", "Effect" },
            { "FontDescriptionProcessor", "SpriteFont" },
            { "TextureProcessor", "Texture2D" },
            { "ModelProcessor", "Model" },
            { "SoundEffectProcessor", "SoundEffect" },
            { "SongProcessor", "Song" },
            { "VideoProcessor", "Video" },
            // BytingLib
            { "BytingFontProcessor", "SpriteFont" },
            { "AnimationProcessor", "Animation" },
            { "StringProcessor", "string" },
            { "GLTFProcessor", "ModelGL" },
            { "BytesProcessor", "byte[]" },
        };

        internal static readonly Dictionary<string, string> ExtensionCopyToDataType = new()
        {
            { "png", "Texture2D" },
            { "jpg", "Texture2D" },
            { "jpeg", "Texture2D" },
            { "bin", "byte[]" },
            { "json", "string" },
            { "txt", "string" },
            { "yaml", "string" },
            // BytingLib
            { "ani", "Animation" },
            { "gltf", "ModelGL" },
        };

        internal static readonly Dictionary<string, string> DataTypeToVarExtension = new()
        {
            { "Effect", "Fx" },
            { "SpriteFont", "Font" },
            { "Texture2D", "Tex" },
            { "Model", "Model" },
            { "SoundEffect", "Sfx" },
            { "Song", "Song" },
            { "Video", "Video" },
            { "byte[]", "Bytes" },
            // BytingLib
            { "Animation", "" }, // Ani is already in the asset name
            { "ModelGL", "Model" }, // Ani is already in the asset name
        };
    }
}
