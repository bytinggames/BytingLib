using BytingLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace BytingPipeline
{
    public class GLTFReader : ContentTypeReader<ModelGL>
    {
        protected override ModelGL Read(ContentReader input, ModelGL existingInstance)
        {
            var gDeviceService = (IGraphicsDeviceService?)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));
            if (gDeviceService == null)
            {
                throw new BytingException("IGraphicsDeviceService is missing");
            }

            GraphicsDevice gDevice = gDeviceService.GraphicsDevice;
            IContentCollectorUse? contentCollector = IContentCollectorUseExtension.CurrentContentCollector; // TODO: when fixing this, also fix it for AnimationReader

            if (contentCollector == null)
            {
                throw new BytingException("IContentCollectorUseExtension.CurrentContentCollector is not set");
            }

            string json = input.ReadString();
            string? gltfDirectory = Path.GetDirectoryName(Path.Combine(input.ContentManager.RootDirectory, input.AssetName));
            if (gltfDirectory == null)
            {
                throw new BytingException("gltfDirectory couldn't be read");
            }

            return new ModelGL(json, gltfDirectory, input.ContentManager.RootDirectory, gDevice, contentCollector);
        }
    }
}