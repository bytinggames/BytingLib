namespace BytingLib.UI
{
    public class SimpleNavigationDrawer : INavigationDrawer
    {
        public Color ColorArea { get; set; } = Color.White * 0.5f;
        public Color ColorOutline { get; set; } = Color.White * 0.75f;
        public float OutlineThickness { get; set; } = 4f;

        public virtual void Draw(SpriteBatch spriteBatch, Element element)
        {
            if (element is ButtonParent)
            {
                // hovering already shows that navigation is on that button
                return;
            }

            element.AbsoluteRect.Draw(spriteBatch, ColorArea);
            if (element is not TextInput)
            {
                DrawOutline(spriteBatch, element.AbsoluteRect.Outline());
            }
        }

        protected void DrawPolygonWithOutline(SpriteBatch spriteBatch, Polygon polygon)
        {
            polygon.Draw(spriteBatch, ColorArea);
            DrawOutline(spriteBatch, polygon.Outline());
        }

        protected void DrawOutline(SpriteBatch spriteBatch, PrimitiveLineRing ring)
        {
            ring.ThickenOutside(OutlineThickness).Draw(spriteBatch, ColorOutline);
        }
    }
}
