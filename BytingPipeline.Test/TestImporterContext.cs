using Microsoft.Xna.Framework.Content.Pipeline;

namespace BytingPipeline.Test
{
    class TestImporterContext : ContentImporterContext
    {
        private readonly TestContentBuildLogger _logger;

        public TestImporterContext()
        {
            _logger = new TestContentBuildLogger();

        }

        public override string IntermediateDirectory => throw new NotImplementedException();

        public override ContentBuildLogger Logger
        {
            get { return _logger; }
        }
        public override string OutputDirectory => throw new NotImplementedException();

        public override void AddDependency(string filename)
        {
            throw new NotImplementedException();
        }
    }
}