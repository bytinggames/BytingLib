using BytingLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "TextureScaleProcessor")]
    public class TextureScaleProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        public int Width { get; set; } = 64;
        public int Height { get; set; } = 64;
        public bool KeepAspectRatio { get; set; } = true;

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            if (Width < 1 || Height < 1)
            {
                return input;
            }

            int outputW = Width;
            int outputH = Height;
            foreach (var mipMapChainCollection in input.Faces)
            {
                for (int faceIndex = 0; faceIndex < mipMapChainCollection.Count; faceIndex++)
                {
                    var face = mipMapChainCollection[faceIndex];
                    int inputW = face.Width;
                    int inputH = face.Height;

                    byte[] bytes = face.GetPixelData();
                    Color[] source = new Color[inputW * inputH];
                    ColorExtension.BytesToColors(bytes, source);
                    Color[] output = new Color[outputW * outputH];
                    ColorExtension.ScaleToTargetSize(ref source, inputW, ref output, outputW, KeepAspectRatio);
                    if (bytes.Length < output.Length * 4)
                    {
                        Array.Resize(ref bytes, output.Length * 4);
                    }
                    ColorExtension.ColorsToBytes(ref output, ref bytes);
                    PixelBitmapContent<Color> newFace = new(outputW, outputH);
                    newFace.SetPixelData(bytes);
                    mipMapChainCollection[faceIndex] = newFace;
                }
            }

            return input;
        }

    }
}