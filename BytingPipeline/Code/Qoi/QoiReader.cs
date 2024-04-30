using BytingLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using QoiSharp;

namespace BytingPipeline
{
    public class QoiReader : ContentTypeReader<Texture2D>
    {
        protected override Texture2D Read(ContentReader input, Texture2D existingInstance)
        {
            var gDeviceService = (IGraphicsDeviceService?)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            if (gDeviceService == null)
            {
                throw new BytingException("IGraphicsDeviceService is missing");
            }
            GraphicsDevice gDevice = gDeviceService.GraphicsDevice;

            QoiTextureContent qoiData = new QoiTextureContent(input);
            var qoiImage = QoiDecoder.Decode(qoiData.Data);
            if (qoiImage.Channels == QoiSharp.Codec.Channels.Rgb)
            {
                throw new System.Exception("QOI RGB loading currently not supported. It is better to use RGBA anyways as the size is the same, if every pixel has 100% alpha. There is no Texture2D RGB format.");
            }

            Texture2D tex = new Texture2D(gDevice, qoiImage.Width, qoiImage.Height);
            tex.SetData(qoiImage.Data);

            return tex;
        }
    }
}