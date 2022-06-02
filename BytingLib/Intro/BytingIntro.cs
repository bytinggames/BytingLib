using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib.Intro
{
    class Tooth
    {
        Polygon polygon;

        public Tooth(Polygon polygon)
        {
            this.polygon = polygon;
        }

        public void Draw(SpriteBatch spriteBatch, float scale, Color color)
        {
            const float defaultLineThickness = 2f;
            const float minLineThickness = 1f;
            const float defaultShadowThickness = 8f;

            float lineThickness = defaultLineThickness;
            float displayedThickness = lineThickness * scale;
            if (displayedThickness < minLineThickness)
                lineThickness *= minLineThickness / displayedThickness;


            //polygon.Draw(spriteBatch, colorBG);
            //polygon.Outline().Thicken(lineThickness, -1f - defaultShadowThickness * 2f /* not sure why *2 is needed */ / lineThickness).Draw(spriteBatch, colorFG);

            // enlarge the polygon and draw it to include the shadow outline
            new PrimitiveAreaFan(polygon.Vertices.Select(f => f + polygon.Pos).ToArray())
                .Enlarge(defaultShadowThickness)
                .Draw(spriteBatch, Color.Transparent);


            polygon.Outline().ThickenInside(lineThickness).Draw(spriteBatch, color);
        }
    }

    public class BytingIntro : IUpdate
    {
        List<Tooth> teeth = new List<Tooth>();

        private readonly Rect requiredSpace;

        RenderTarget2D? renderTarget;

        private bool firstDraw = true;

        public Texture2D DrawOnMyOwn(SpriteBatch spriteBatch, Int2 size, Color colorFG)
        {
            // somehow there must be a draw once before the rendertarget gets created, or otherwise the render target gets pixelated...
            if (firstDraw)
            {
                firstDraw = false;
                spriteBatch.Begin();
                spriteBatch.Draw(spriteBatch.GetPixel(), Vector2.Zero, Color.Transparent);
                spriteBatch.End();
            }
            if (renderTarget == null || renderTarget.Width != size.X || renderTarget.Height != size.Y)
                RefreshRenderTarget(spriteBatch.GraphicsDevice, size);

            float wScale = size.X / requiredSpace.Width;
            float hScale = size.Y / requiredSpace.Height;
            float scale = MathF.Min(wScale, hScale);
            Vector2 offset = size.ToVector2() / 2 - requiredSpace.GetCenter();
            Matrix transform = Matrix.CreateScale(scale) 
                * Matrix.CreateTranslation(new Vector3(offset, 0f));

            spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            // blend state that replaces all old drawn pixels with the new pixel color
            var blendState = new BlendState()
            {
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
            };
            spriteBatch.Begin(blendState: blendState, transformMatrix: transform);

            foreach (var tooth in teeth)
            {
                tooth.Draw(spriteBatch, scale, colorFG);
            }

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            return renderTarget!;
        }

        private void RefreshRenderTarget(GraphicsDevice gDevice, Int2 size)
        {
            if (renderTarget != null)
                renderTarget.Dispose();
            renderTarget = new RenderTarget2D(gDevice, size.X, size.Y, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
        }

        public BytingIntro()
        {
            const float teethSize = 100;

            for (int x = 0; x < 8; x++)
            {
                Vector2 pos = new Vector2((x - 3.5f) * teethSize * 0.6f, 0);

                teeth.Add(new Tooth(
                    Polygon.GetRandomConvex(pos, new Random(), teethSize / 2f).TransformVertices(Matrix.CreateScale(1f, 1.5f, 1f))
                    ));
            }

            requiredSpace = Anchor.Center(Vector2.Zero).Rectangle(1920, 1080);
        }

        public void Update()
        {

        }
    }
}
