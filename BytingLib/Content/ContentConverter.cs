
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace BytingLib
{
    public class ContentConverter
    {
        public Dictionary<string, string> ProcessorToDataType { get; } = new()
        {
            { "EffectProcessor", "Effect" },
            { "FontDescriptionProcessor", "SpriteFont" },
            { "TextureProcessor", "Texture2D" },
            { "QoiProcessor", "Texture2D" },
            { "ModelProcessor", "Model" },
            { "SoundEffectProcessor", "SoundEffect" },
            { "SongProcessor", "Song" },
            { "FontTextureProcessor", "SpriteFont" },
            // BytingLib
            { "BytingFontProcessor", "SpriteFont" },
            { "AnimationProcessor", "Animation" },
            { "StringProcessor", "string" },
            { "GLTFProcessor", "ModelGL" },
            { "BytesProcessor", "byte[]" },
        };

        public Dictionary<string, string> ExtensionCopyToDataType { get; } = new()
        {
            { "png", "Texture2D" },
            { "jpg", "Texture2D" },
            { "jpeg", "Texture2D" },
            { "qoi", "Texture2D" },
            { "bin", "byte[]" },
            { "json", "string" },
            { "txt", "string" },
            { "yaml", "string" },
            // BytingLib
            { "ani", "Animation" },
            { "gltf", "ModelGL" },
            { "loca", "Localization" },
        };

        public Dictionary<string, string> DataTypeToVarExtension { get; } = new()
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

        public Dictionary<string, Type> RuntimeTypes = new()
        {
            { "Effect", typeof(Effect) },
            { "SpriteFont", typeof(SpriteFont) },
            { "Texture2D", typeof(Texture2D) },
            { "Model", typeof(Model) },
            { "SoundEffect", typeof(SoundEffect) },
            { "Song", typeof(Song) },
            // BytingLib
            { "Animation", typeof(Animation) },
            { "string", typeof(string) },
            { "ModelGL", typeof(ModelGL) },
            { "byte[]", typeof(byte[]) },
        };
    }
}
