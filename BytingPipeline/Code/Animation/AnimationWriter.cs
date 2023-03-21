using BytingLib;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace BytingPipeline
{
    [ContentTypeWriter]
    public class AnimationWriter : ContentTypeWriter<AnimationContent>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AnimationReader).AssemblyQualifiedName ?? "";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Animation).AssemblyQualifiedName ?? "";
        }

        protected override void Write(ContentWriter output, AnimationContent value)
        {
            output.Write(value.Json);
        }
    }
}