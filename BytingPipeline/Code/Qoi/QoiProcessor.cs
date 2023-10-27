using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using QoiSharp;
using System;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "QoiProcessor")]
    public class QoiProcessor : ContentProcessor<TextureContent, QoiTextureContent>
    {
        public override QoiTextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            if (input.Faces.Count == 0
                || input.Faces[0].Count == 0)
            {
                throw new NotImplementedException("texture content should at least have one face");
            }

            var face = input.Faces[0];
            QoiImage qoiImage = new QoiImage(face[0].GetPixelData(), face[0].Width, face[0].Height, QoiSharp.Codec.Channels.RgbWithAlpha);
            byte[] qoi = QoiEncoder.Encode(qoiImage);

            return new QoiTextureContent(qoi);
        }
    }
}