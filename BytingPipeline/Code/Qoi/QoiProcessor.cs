using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using QoiSharp;
using System;
using System.ComponentModel;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "QoiProcessor")]
    public class QoiProcessor : ContentProcessor<TextureContent, QoiTextureContent>
    {
        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; } = true;

        public override QoiTextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            if (input.Faces.Count == 0
                || input.Faces[0].Count == 0)
            {
                throw new NotImplementedException("texture content should at least have one face");
            }

            var face = input.Faces[0];
            var pixelData = face[0].GetPixelData();
            if (PremultiplyAlpha)
            {
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    int a = pixelData[i + 3];
                    if (a != 255)
                    {
                        // Premultiply Aplha (from Color.FromNonPremultiplied)
                        pixelData[i] = (byte)(pixelData[i] * a / 255);
                        pixelData[i + 1] = (byte)(pixelData[i + 1] * a / 255);
                        pixelData[i + 2] = (byte)(pixelData[i + 2] * a / 255);
                    }
                }
            }
            QoiImage qoiImage = new QoiImage(pixelData, face[0].Width, face[0].Height, QoiSharp.Codec.Channels.RgbWithAlpha);

            byte[] qoi = QoiEncoder.Encode(qoiImage);

            return new QoiTextureContent(qoi);
        }
    }
}