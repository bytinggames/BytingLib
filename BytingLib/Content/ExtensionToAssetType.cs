using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class ExtensionToAssetType
    {
        public static Type? Convert(string localPath)
        {
            string extension = Path.GetExtension(localPath);

            if (string.IsNullOrEmpty(extension))
                return null;
            if (extension[0] == '.')
                extension = extension[1..];

            Type? assetType = extension switch
            {
                "png" or "jpeg" or "jpg" => typeof(Texture2D),
                "spritefont" => typeof(SpriteFont),
                "wav" or "mp3" or "ogg" => typeof(SoundEffect),
                "fx" => typeof(Effect),
                "fbx" => typeof(Model),
                "ani" => typeof(AnimationData),
                "csv" or "txt" or "ini" or "config" or "xml" or "json" => typeof(string),
                _ => null,
            };
            return assetType;
        }
    }
}
