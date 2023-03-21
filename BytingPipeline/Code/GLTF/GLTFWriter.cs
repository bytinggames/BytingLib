using BytingLib;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace BytingPipeline
{
    [ContentTypeWriter]
    public class GLTFWriter : ContentTypeWriter<GLTFContentJson>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(GLTFReader).AssemblyQualifiedName ?? "";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(ModelGL).AssemblyQualifiedName ?? "";
        }

        protected override void Write(ContentWriter output, GLTFContentJson value)
        {
            output.Write(value.Json);
        }
    }
}