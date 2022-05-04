
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class SpriteFontExtension
    {
        public static string StringReplaceUnresolvableCharacters(this SpriteFont font, string text, string replaceWith)
        {
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (!font.Characters.Contains(text[i]))
                    text = text.Remove(i, 1).Insert(i, replaceWith);
            }
            return text;
        }
    }
}
