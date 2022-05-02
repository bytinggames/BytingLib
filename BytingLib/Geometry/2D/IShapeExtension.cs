
using Microsoft.Xna.Framework;

namespace BytingLib
{
    public static class IShapeExtension
    {
        public static Vector2 GetCenter(this IShape shape)
        {
            return shape.GetBoundingRectangle().GetCenter();
        }
    }
}
