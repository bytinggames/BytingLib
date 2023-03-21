using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline
{
    [ContentProcessor(DisplayName = "AnimationProcessor")]
    internal class AnimationProcessor : ContentProcessor<string, AnimationContent>
    {
        public override AnimationContent Process(string json, ContentProcessorContext context)
        {
            return new AnimationContent(json);
        }
    }
}