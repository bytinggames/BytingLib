using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public abstract class PrimitiveArea
    {
        public abstract void Draw(GraphicsDevice gDevice);

        public abstract PrimitiveLineRing Outline();

        public PrimitiveAreaRing Outline(float thickness)
        {
            return Outline().Thicken(thickness);
        }
        public PrimitiveAreaRing Outline(float thickness, float anchor = 0f)
        {
            return Outline().Thicken(thickness, anchor);
        }
        public PrimitiveAreaRing OutlineInside(float thickness)
        {
            return Outline().ThickenInside(thickness);
        }
        public PrimitiveAreaRing OutlineOutside(float thickness)
        {
            return Outline().ThickenOutside(thickness);
        }
    }
}
