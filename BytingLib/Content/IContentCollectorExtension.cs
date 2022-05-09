using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public static class IContentCollectorExtension
    {
        public static Animation UseAnimation(this IContentCollector contentCollector, string assetName)
        {
            Ref<Texture2D> tex = contentCollector.Use<Texture2D>(assetName);
            Ref<AnimationData> animationData = contentCollector.Use<AnimationData>(assetName + ".ani");
            return new Animation(tex, animationData);
        }
    }
}