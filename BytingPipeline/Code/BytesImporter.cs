using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace BytingPipeline
{
    [ContentImporter(".bin", DisplayName = "BytesImporter", DefaultProcessor = "BytesProcessor")]
    public class BytesImporter : ContentImporter<byte[]>
    {
        public override byte[] Import(string filename, ContentImporterContext context)
        {
            return File.ReadAllBytes(filename);
        }
    }
}