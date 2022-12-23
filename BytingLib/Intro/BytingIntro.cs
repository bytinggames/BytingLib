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
        public IList<Vector2> Vertices { get; set; }
        [BytingProp(1)]
        public bool Open { get; set; }
        [BytingProp(2)]
        public bool Shadow { get; set; }
        [BytingProp(3)]
        public ThickVertex[] ThickVertices { get; set; }
        public Vector2 Move { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Tooth()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public Tooth(IList<Vector2> vertices, bool open, bool shadow, ThickVertex[] thickVertices)
        {
            this.Vertices = vertices;
            this.Open = open;
            this.Shadow = shadow;
            this.ThickVertices = thickVertices;
        }

        public void Draw(SpriteBatch spriteBatch, float scale, Color color)
        {
            const float thicknessScale = 1.3f;

            const float defaultLineThickness = 8f * thicknessScale; // 6 previously. 8 for the website
            const float minLineThickness = 1f * thicknessScale;
            const float defaultShadowThickness = defaultLineThickness + 9f * thicknessScale;
            const float thickerThickness = 16f * thicknessScale; // 12 previously. 16 for the website

            float lineThickness = defaultLineThickness;
            float displayedThickness = lineThickness * scale;
            if (displayedThickness < minLineThickness)
                lineThickness *= minLineThickness / displayedThickness;

            if (Shadow)
            {
                new PrimitiveAreaFan(Vertices.ToArray())
                    .Enlarge(defaultShadowThickness)
                    .Draw(spriteBatch, Color.Transparent);
            }


            // enlarge the polygon and draw it to include the shadow outline
            if (Open)
            {
                var strip = new PrimitiveLineStrip(Vertices);
                //strip.ThickenOutside(defaultShadowThickness)
                //    .Draw(spriteBatch, Color.Transparent);

                strip.ThickenOutside(lineThickness).Draw(spriteBatch, color);
            }
            else
            {
                new PrimitiveLineRing(Vertices).ThickenOutside(lineThickness).Draw(spriteBatch, color);
            }

            foreach (var t in ThickVertices)
            {
                int indexJau = t.Index + 2 * (t.DirectionForward ? 1 : -1);
                if (Vertices.Count > 2 && (!Open || indexJau >= 0 && indexJau < Vertices.Count))
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
            index %= Vertices.Count;
            if (index < 0)
                index += Vertices.Count;
            return Vertices[index];
        }

        public void Dispose()
        {
            Vertices.Clear();
            ThickVertices = new ThickVertex[0];
        }
    }
    class IntroData : IDisposable
    {
        [BytingProp(0)]
        public List<Tooth> Teeth { get; set; } = new List<Tooth>();

        public void Dispose()
        {
            Teeth.ForEach(f => f.Dispose());
            Teeth.Clear();
        }
    }

    public class BytingIntro : IUpdate, IDisposable
    {
        static readonly bool edit = false;
        static readonly bool animate = false;
        private static readonly string introDataFile = Path.Combine("..", "..", "..", "intro.bin");
        IntroData data = new IntroData();

        private readonly Rect requiredSpace;
        private readonly MouseInput mouse;
        private readonly KeyInput keys;
        RenderTarget2D? renderTarget;

        private bool firstDraw = true;
        Serializer serializer;

        Tooth? selectedTooth;
        int? selectedVertex;

        public BytingIntro(MouseInput mouse, KeyInput keys)
        {
            this.mouse = mouse;
            this.keys = keys;

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
                data = serializer.Deserialize<IntroData>(fs)!;

            Vector2 center;

            if (animate)
            {
                data.Teeth.RemoveRange(16, data.Teeth.Count - 16);

                for (int y = 0; y < 2; y++)
                {
                    Vector2 baseLine = Rect.FromPoints(data.Teeth[y * 8 + 7].Vertices)!.GetCenter();
                    for (int x = 0; x < 8; x++)
                    {
                        var tooth = data.Teeth[y * 8 + 7 - x];
                        center = Rect.FromPoints(tooth.Vertices)!.GetCenter();
                        tooth.Move = -(baseLine - center + new Vector2(140 * (x + 1), 0));
                        for (int i = 0; i < tooth.Vertices.Count; i++)
                        {
                            tooth.Vertices[i] -= tooth.Move;
                        }
                    }
                }
            }

            //Rect bytingGamesRect = Rect.FromPoints(data.Teeth.Take(16).SelectMany(f => f.Vertices))!;
            //center = bytingGamesRect.GetCenter();
            //center.X += 200;
            //for (int i = 0; i < 16; i++)
            //{
            //    for (int j = 0; j < data.Teeth[i].Vertices.Count; j++)
            //    {
            //        Vector2 pos = data.Teeth[i].Vertices[j] + new Vector2(0,-10);
            //        data.Teeth[i].Vertices[j] = pos;
            //    }
            //}

            //bytingGamesRect = Rect.FromPoints(data.Teeth.Skip(16).SelectMany(f => f.Vertices))!;
            //center = bytingGamesRect.GetCenter();
            //center.X -= 200;
            //for (int i = 16; i < data.Teeth.Count; i++)
            //{
            //    for (int j = 0; j < data.Teeth[i].Vertices.Count; j++)
            //    {
            //        Vector2 dist = data.Teeth[i].Vertices[j] - center;
            //        data.Teeth[i].Vertices[j] += dist * 0.3f;
            //    }
            //}
        }

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




            if (edit)
            {
                Matrix toInput = Matrix.Invert(transform);
                UpdateDraw(toInput);
            }
            spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            // blend state that replaces all old drawn pixels with the new pixel color
            var blendState = new BlendState()
            {
                AlphaDestinationBlend = Blend.Zero,
                ColorDestinationBlend = Blend.Zero,
            };
            spriteBatch.Begin(blendState: blendState, transformMatrix: transform);

            foreach (var tooth in data.Teeth)
            {
                tooth.Draw(spriteBatch, scale, colorFG);
            }
            //if (!mouse.Left.Down)
            //    selectedTooth?.Draw(spriteBatch, scale, Color.Yellow);

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);



            return renderTarget!;
        }

        private void UpdateDraw(Matrix toInput)
        {
            Vector2 mousePos = Vector2.Transform(mouse.Position, toInput);
            Vector2 mouseMove = mousePos - Vector2.Transform(mouse.GetStatePrevious().Position.ToVector2(), toInput);

            if ((mouse.Right.Down || mouse.Left.Down) && selectedTooth != null)
            {
                if (mouse.Move != Vector2.Zero)
                {
                    if (mouse.Left.Down)
                    {
                        for (int i = 0; i < selectedTooth.Vertices.Count; i++)
                        {
                            selectedTooth.Vertices[i] += mouseMove;
                        }
                    }
                    else
                    {
                        selectedTooth.Vertices[selectedVertex!.Value] += mouseMove;
                    }
                }
            }
            else
            {

                //if (mouse.Right.Pressed || mouse.Left.Pressed)
                {
                    float minDist = float.MaxValue;

                    foreach (var tooth in data.Teeth)
                    {
                        for (int i = 0; i < tooth.Vertices.Count; i++)
                        {
                            float dist = (tooth.Vertices[i] - mousePos).LengthSquared();
                            if (dist < minDist)
                            {
                                minDist = dist;
                                selectedTooth = tooth;
                                selectedVertex = i;
                            }
                        }
                    }
                }
            }
        }

        private void RefreshRenderTarget(GraphicsDevice gDevice, Int2 size)
        {
            if (renderTarget != null)
                renderTarget.Dispose();
            renderTarget = new RenderTarget2D(gDevice, size.X, size.Y, false, SurfaceFormat.Color, DepthFormat.None, 8, RenderTargetUsage.DiscardContents);
        }

        float time = 0f;
        float animTimePrevious = 0f;

        public void Update()
        {
            if (!animate)
                return;

            time += 0.01f;

            float animTime = Curves.EaseInOutQuad(time);

            float timeDelta = animTime - animTimePrevious;
            animTimePrevious = animTime;


            if (time <= 1f)
            {
                foreach (var tooth in data.Teeth)
                {
                    for (int i = 0; i < tooth.Vertices.Count; i++)
                    {
                        tooth.Vertices[i] += tooth.Move * timeDelta;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (edit)
            {
                File.Move(introDataFile, introDataFile + DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss_fff"));
                using (var fs = File.Create(introDataFile))
                    serializer.Serialize(fs, data);
            }

            renderTarget?.Dispose();

            data.Dispose();
        }
    }
}
