using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace BytingPipeline
{
    [ContentImporter(".json|.txt|.yaml|*.gltf", DisplayName = "StringImporter", DefaultProcessor = "StringProcessor")]
    public class StringImporter : ContentImporter<string>
    {
        public override string Import(string filename, ContentImporterContext context)
        {
            return File.ReadAllText(filename);
        }
    }
}