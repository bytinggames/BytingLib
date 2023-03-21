using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "BytesProcessor")]
    internal class BytesProcessor : ContentProcessor<byte[], byte[]>
    {
        public override byte[] Process(byte[] input, ContentProcessorContext context)
        {
            return input;
        }
    }
}