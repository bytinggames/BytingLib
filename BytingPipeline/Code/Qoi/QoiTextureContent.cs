using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace BytingPipeline
{
    public class QoiTextureContent
    {
        public byte[] Data { get; set; }

        public QoiTextureContent(byte[] data)
        {
            Data = data;
        }

        public QoiTextureContent(ContentReader input)
        {
            int dataLength = input.ReadInt32();
            Data = input.ReadBytes(dataLength);
        }

        public void Write(ContentWriter output)
        {
            output.Write(Data.Length);
            output.Write(Data);
        }
    }
}