using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "QoiBytesProcessor")]
    public class QoiBytesProcessor : ContentProcessor<byte[], QoiTextureContent>
    {
        public override QoiTextureContent Process(byte[] qoiData, ContentProcessorContext context)
        {
            return new QoiTextureContent(qoiData);
        }
    }
}