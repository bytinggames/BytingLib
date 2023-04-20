using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "Sprite Font Description - Byting Games")]
    public class BytingFontProcessor : FontDescriptionProcessor
    {
        [DefaultValue(0)]
        public virtual int Thickness { get; set; } = 0;

        protected override int GetGlobalOffsetX() => -Thickness;

        protected override void ModifyGlyphs(HashSet<GlyphData> glyphData)
        {
            if (Thickness > 0)
            {
                ModifyGlyphsThickness(glyphData);
            }
        }

        private void ModifyGlyphsThickness(HashSet<GlyphData> glyphData)
        {
            List<(int x, int y)> search = new();

            int maxDistanceSquared = Thickness * Thickness;
            for (int y = -Thickness; y <= Thickness; y++)
            {
                for (int x = -Thickness; x <= Thickness; x++)
                {
                    int distanceSquared = x * x + y * y;
                    if (distanceSquared <= maxDistanceSquared)
                        search.Add((x, y));
                }
            }

            foreach (GlyphData glyph in glyphData)
            {
                GlyphCropper.Enlarge(glyph, Thickness);

                int newWidth = glyph.Bitmap.Width + Thickness * 2;
                int newHeight = glyph.Bitmap.Height + Thickness * 2;

                byte[] dataSource = glyph.Bitmap.GetPixelData();
                byte[] dataTarget = new byte[newWidth * newHeight];
                for (int y1 = 0; y1 < glyph.Bitmap.Height; y1++)
                {
                    for (int x1 = 0; x1 < glyph.Bitmap.Width; x1++)
                    {
                        int x2 = x1 + Thickness;
                        int y2 = y1 + Thickness;
                        int i1 = y1 * glyph.Bitmap.Width + x1;
                        int i2 = y2 * newWidth + x2;
                        dataTarget[i2] = dataSource[i1];
                    }
                }

                var newBitmap = (BitmapContent?)Activator.CreateInstance(glyph.Bitmap.GetType(), new object[] { newWidth, newHeight });
                if (newBitmap != null)
                {
                    BitmapContent.Copy(glyph.Bitmap, newBitmap);
                    glyph.Bitmap = newBitmap;

                    dataSource = dataTarget;
                    dataTarget = new byte[dataTarget.Length];

                    int w = glyph.Bitmap.Width;
                    int h = glyph.Bitmap.Height;

                    for (int y = 0; y < glyph.Bitmap.Height; y++)
                    {
                        for (int x = 0; x < glyph.Bitmap.Width; x++)
                        {
                            byte darkest = 0;
                            foreach (var s in search)
                            {
                                int xSearch = x + s.x;
                                int ySearch = y + s.y;
                                if (xSearch >= 0 && xSearch < w
                                    && ySearch >= 0 && ySearch < h)
                                {
                                    darkest = Math.Max(darkest, dataSource[ySearch * w + xSearch]);
                                }
                            }

                            int i = y * w + x;

                            dataTarget[i] = darkest;
                        }
                    }

                    glyph.Bitmap.SetPixelData(dataTarget);
                }
            }
        }
    }
}