using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "StringProcessor")]
    internal class StringProcessor : ContentProcessor<string, string>
    {
        public override string Process(string str, ContentProcessorContext context)
        {
            return str;
        }
    }
}