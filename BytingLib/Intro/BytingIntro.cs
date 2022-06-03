using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib.Intro
{
    class ThickVertex
    {
        public int Index { get; }
        public bool DirectionForward { get; }

        public ThickVertex(int index, bool directionForward)
        {
            Index = index;
            DirectionForward = directionForward;
        }
    }

    class Tooth
    {
        IList<Vector2> vertices;
        private readonly bool open;
        private readonly bool shadow;
        private readonly ThickVertex[] thickVertices;

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

            //1531;426 1545;410 1562;413 1589;461 1588;477 1562;504 
            string str = @"
1695;366 1668;288 1606;282 1577;308 1582;371 1635;412 
1728;401 1720;321 1667;318 1642;351 1645;430 
1747;484 1773;318 1677;390 
1733;486 1721;405 1662;406 1613;456 1636;549 1692;549 
1656;497 1640;444 1606;441 1553;509 1569;578 1610;581 
1648;628 1713;428 1614;489 
1620;580 1626;509 1574;471 1521;507 1521;598 1559;633 1597;623 
1536;573 1527;500 1487;483 1430;507 1454;603 1474;637 1507;635 
";

            Vector2[] offsets = new Vector2[]
                {
                    new Vector2(253,212),
                    new Vector2(272,229),
                    new Vector2(281,273),
                    new Vector2(275,272),
                    new Vector2(236,280),
                    new Vector2(170,333),
                    new Vector2(155,310),
                    new Vector2(112,296),
                };

            Vector2 totalOffset = new Vector2(-700, -300);

            CreateTeeth(str, offsets, totalOffset, true);

            str = @"
2231;349 2217;453 2185;465 2146;435 2157;335 2193;321 
2282;352 2273;500 2215;435 
2278;341 2259;415 2212;426 2174;385 2202;304 2254;288 
2245;311 2228;405 2183;412 2142;366 2156;273 2213;251 
2184;309 2167;377 2112;375 2078;268 2114;230 2163;255 
2093;280 2085;363 2025;367 1995;269 2032;225 2076;236 
1987;228 2011;434 1918;360 
1922;319 1888;391 1810;391 1792;269 1816;226 1869;219 1921;247 
";

            offsets = new Vector2[]
                {
                    new Vector2(258,81),
                    new Vector2(282,81),
                    new Vector2(280,74),
                    new Vector2(265,57),
                    new Vector2(234,58),
                    new Vector2(188,45),
                    new Vector2(133,16),
                    new Vector2(102,64),
                };

            CreateTeeth(str, offsets, totalOffset, false);


            str = @"
1855;276 1838;116 1947;133 1949;169 1920;195 1954;200 1958;245 1922;267 
1969;318 2037;160 
2001;243 1974;159 
2065;129 2068;271 2089;278 2118;265 
2123;170 2051;176 
2149;162 2153;277 
2148;120 2148;151 
2195;267 2188;172 
2192;188 2215;168 2236;167 2251;186 2249;263 
2346;254 2329;264 2300;262 2268;236 2286;156 2339;152 2359;161 2364;264 2351;299 2322;313 2283;305 
1869;552 1942;543 1942;604 1911;630 1859;631 1817;569 1834;489 1882;464 1940;482 
1966;515 2032;509 2052;543 2039;627 2017;613 1982;627 1955;601 1956;562 2025;548 
2067;625 2064;531 2099;510 2125;528
2115;622 2125;528 2156;520 2168;537 2165;622 
2278;608 2237;628 2201;613 2183;555 2215;514 2250;502 2281;532 2283;574 2213;562 
2378;524 2355;505 2310;510 2298;541 2327;567 2363;563 2375;605 2341;622 2291;615 
";
            totalOffset = new Vector2(-1700, -330);

            CreateLines(str, totalOffset);

            //for (int y = -1; y <= 1; y += 2)
            //{
            //    for (int x = 0; x < 8; x++)
            //    {
            //        Vector2 pos = new Vector2((x - 3.5f) * teethSize * 0.8f, y * teethSize * 2f);

            //        teeth.Add(new Tooth(
            //            Polygon.GetRandomConvex(pos, new Random(), teethSize / 2f).TransformVertices(Matrix.CreateScale(1f, 1.5f, 1f)).Vertices.Select(f => f + pos).ToList()
            //            , false, 0, true));
            //    }
            //}

            requiredSpace = Anchor.Center(Vector2.Zero).Rectangle(1920, 1080);
        }

        private void CreateTeeth(string str, Vector2[] offsets, Vector2 totalOffset, bool reverse)
        {
            string[] teethStr = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var s in teethStr)
            {
                string[] split = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<Vector2> vertices = new List<Vector2>();
                foreach (var s2 in split)
                {
                    string[] split2 = s2.Split(new char[] { ';' });
                    Vector2 v = new Vector2(float.Parse(split2[0]), float.Parse(split2[1]));
                    vertices.Add(v);
                }

                Rect rect = Rect.FromPoints(vertices)!;
                Vector2 center = rect.GetCenter();
                for (int j = 0; j < vertices.Count; j++)
                {
                    vertices[j] = center + (vertices[j] - center) * 0.94f;
                }

                Vector2 start = vertices[0];
                if (reverse)
                {
                    vertices.Reverse();
                }



                teeth.Add(new Tooth(vertices.Select(f => f - start + offsets[i] * 2f + totalOffset).ToList(), vertices.Count == 3, true, new ThickVertex[] { new ThickVertex(reverse ? vertices.Count - 1 : 0, !reverse) }));

                i++;
            }
        }

        private void CreateLines(string str, Vector2 totalOffset)
        {
            string[] teethStr = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var s in teethStr)
            {
                string[] split = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<Vector2> vertices = new List<Vector2>();
                foreach (var s2 in split)
                {
                    string[] split2 = s2.Split(new char[] { ';' });
                    Vector2 v = new Vector2(float.Parse(split2[0]), float.Parse(split2[1]));
                    vertices.Add(v);
                }

                bool open = true;

                List<ThickVertex> thicks = new List<ThickVertex>();

                if (i == 0)
                    open = false;
                switch (i)
                {
                    case 0: // B
                        open = false;
                        thicks.Add(new ThickVertex(0, true));
                        thicks.Add(new ThickVertex(4, false));
                        //thicks.Add(new ThickVertex(7, false));
                        break;
                    case 1: // y1
                        thicks.Add(new ThickVertex(0, true));
                        break;
                    case 2: // y2
                        thicks.Add(new ThickVertex(0, true));
                        break;
                    case 3: // t1
                        thicks.Add(new ThickVertex(1, false));
                        break;
                    case 4: // t2
                        break;
                    case 5: // i1
                        thicks.Add(new ThickVertex(1, false));
                        break;
                    case 6: // i2
                        thicks.Add(new ThickVertex(1, false));
                        break;
                    case 7: // n1
                        thicks.Add(new ThickVertex(0, true));
                        break;
                    case 8: // n2
                        thicks.Add(new ThickVertex(4, false));
                        break;
                    case 9: // g
                        thicks.Add(new ThickVertex(3, true));
                        thicks.Add(new ThickVertex(7, false));
                        break;
                    case 10: // G
                        thicks.Add(new ThickVertex(2, false));
                        thicks.Add(new ThickVertex(5, true));
                        break;
                    case 11: // a
                        thicks.Add(new ThickVertex(3, false));
                        thicks.Add(new ThickVertex(6, true));
                        break;
                    case 12: // m1
                        thicks.Add(new ThickVertex(0, true));
                        break;
                    case 13: // m2
                        thicks.Add(new ThickVertex(0, true));
                        //thicks.Add(new ThickVertex(4, false));
                        break;
                    case 14: // e
                        thicks.Add(new ThickVertex(2, true));
                        thicks.Add(new ThickVertex(7, false));
                        break;
                    case 15: // s1
                        thicks.Add(new ThickVertex(3, false));
                        thicks.Add(new ThickVertex(6, false));
                        break;
                    //case 16: // s2
                    //    thicks.Add(new ThickVertex(2, false));
                    //    break;
                }

                // TODO: a bit more offset between the letters probably

                teeth.Add(new Tooth(vertices.Select(f => f + totalOffset).ToList(), open, false, thicks.ToArray()));

                i++;
            }
        }

        public void Update()
        {

        }
    }
}
