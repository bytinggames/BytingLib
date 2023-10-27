using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;

namespace BytingPipeline
{
    [ContentTypeWriter]
    public class QoiWriter : ContentTypeWriter<QoiTextureContent>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(QoiReader).AssemblyQualifiedName ?? "";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Texture2D).AssemblyQualifiedName ?? "";
        }

        protected override void Write(ContentWriter output, QoiTextureContent value)
        {
            value.Write(output);
        }
    }
}