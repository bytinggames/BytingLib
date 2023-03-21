using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "GLTFProcessor")]
    internal class GLTFProcessor : ContentProcessor<string, GLTFContentJson>
    {
        public override GLTFContentJson Process(string json, ContentProcessorContext context)
        {
            return new GLTFContentJson(json);
        }
    }
}