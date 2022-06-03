using BytingLib.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib.Intro
{
    class ThickVertex
    {
        [BytingProp(0)]
        public int Index { get; set; }
        [BytingProp(1)]
        public bool DirectionForward { get; set; }

        public ThickVertex()
        {
        }

        public ThickVertex(int index, bool directionForward)
        {
            Index = index;
            DirectionForward = directionForward;
        }
    }

    class Tooth
    {

        [BytingProp(0)]
        public IList<Vector2> vertices { get; set; }
        [BytingProp(1)]
        public bool open { get; set; }
        [BytingProp(2)]
        public bool shadow { get; set; }
        [BytingProp(3)]
        public ThickVertex[] thickVertices { get; set; }

        public Tooth()
        {

        }

        public Tooth(IList<Vector2> vertices, bool open, bool shadow, ThickVertex[] thickVertices)
        {
            this.vertices = vertices;
            this.open = open;
            this.shadow = shadow;
            this.thickVertices = thickVertices;
        }

        public void Draw(SpriteBatch spriteBatch, float scale, Color color)
        {
            const float defaultLineThickness = 4f;
            const float minLineThickness = 1f;
            const float defaultShadowThickness = defaultLineThickness + 8f;
            const float thickerThickness = 9f; // 9 seems right on full hd

            float lineThickness = defaultLineThickness;
            float displayedThickness = lineThickness * scale;
            if (displayedThickness < minLineThickness)
                lineThickness *= minLineThickness / displayedThickness;

            if (shadow)
            {
                new PrimitiveAreaFan(vertices.ToArray())
                    .Enlarge(defaultShadowThickness)
                    .Draw(spriteBatch, Color.Transparent);
            }


            // enlarge the polygon and draw it to include the shadow outline
            if (open)
            {
                var strip = new PrimitiveLineStrip(vertices);
                //strip.ThickenOutside(defaultShadowThickness)
                //    .Draw(spriteBatch, Color.Transparent);

                strip.ThickenOutside(lineThickness).Draw(spriteBatch, color);
            }
            else
            {
                new PrimitiveLineRing(vertices).ThickenOutside(lineThickness).Draw(spriteBatch, color);
            }

            foreach (var t in thickVertices)
            {
                int indexJau = t.Index + 2 * (t.DirectionForward ? 1 : -1);
                if (vertices.Count > 2 && (!open || indexJau >= 0 && indexJau < vertices.Count))
                {
                    Vector2 a = GetWrapped(t.Index);
                    Vector2 b = GetWrapped(t.Index + 1 * (t.DirectionForward ? 1 : -1));
                    Vector2 c = GetWrapped(t.Index + 2 * (t.DirectionForward ? 1 : -1));
                    Vector2 ab = b - a;
                    Vector2 bc = c - b;
                    Vector2 orth = Vector2.Normalize(ab).GetRotate90();
                    if (!t.DirectionForward)
                        orth = -orth;
                    Vector2 maxWidthPoint = b + orth * thickerThickness;
                    Vector2 aToMaxWidth = maxWidthPoint - a;

                    float col = Collision.GetCollisionOf2Axes(a, aToMaxWidth, b, bc);

                    Vector2 thickCorner = a + aToMaxWidth * col;

                    if (!t.DirectionForward)
                        CodeHelper.Swap(ref a, ref b);

                    spriteBatch.DrawTriangle(spriteBatch.GetPixel(), a, b, thickCorner, color);
                }
                else
                {
                    Vector2 a = GetWrapped(t.Index);
                    Vector2 b = GetWrapped(t.Index + 1 * (t.DirectionForward ? 1 : -1));
                    Vector2 ab = b - a;
                    Vector2 orth = Vector2.Normalize(ab).GetRotate90();
                    if (!t.DirectionForward)
                        orth = -orth;
                    Vector2 maxWidthPoint = b + orth * thickerThickness;

                    if (!t.DirectionForward)
                        CodeHelper.Swap(ref a, ref b);

                    spriteBatch.DrawTriangle(spriteBatch.GetPixel(), a, b, maxWidthPoint, color);
                }
            }
        }

        private Vector2 GetWrapped(int index)
        {
            index %= vertices.Count;
            if (index < 0)
                index += vertices.Count;
            return vertices[index];
        }
    }
    class IntroData
    {
        [BytingProp(0)]
        public List<Tooth> teeth { get; set; } = new List<Tooth>();

    }


    public class BytingIntro : IUpdate, IDisposable
    {
        private const string introDataFile = @"..\..\..\intro.bin";
        IntroData intro = new IntroData();

        private readonly Rect requiredSpace;

        RenderTarget2D? renderTarget;

        private bool firstDraw = true;
        Serializer serializer;

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

            foreach (var tooth in intro.teeth)
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

            requiredSpace = Anchor.Center(Vector2.Zero).Rectangle(1920, 1080);

            serializer = new Serializer(new TypeIDs(new Dictionary<Type, int>()
            {
                {typeof(IntroData), 0 },
                {typeof(Tooth), 1 },
                {typeof(ThickVertex), 2 },
                {typeof(List<Tooth>), 3 },
                {typeof(List<Vector2>), 4 },
            }), false);

            using (var fs = File.OpenRead(introDataFile))
                intro = serializer.Deserialize<IntroData>(fs)!;
        }

        public void Update()
        {

        }

        public void Dispose()
        {
            using (var fs = File.Create(introDataFile))
                serializer.Serialize(fs, intro);

        }
    }
}
