using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class ExtensionToAssetType
    {
        public static Type? Convert(string extension)
        {
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
                _ => null,
            };
            return assetType;
        }
    }
}
