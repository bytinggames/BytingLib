namespace BytingLib
{
    /// <summary>
    /// Used for loading GlModels. They need access to loading content and to the graphics device.
    /// </summary>
    public struct ExtendedLoadParameter
    {
        public IContentCollectorUse ContentCollector;
        public GraphicsDevice GraphicsDevice;

        public ExtendedLoadParameter(IContentCollectorUse contentCollector, GraphicsDevice graphicsDevice)
        {
            ContentCollector = contentCollector;
            GraphicsDevice = graphicsDevice;
        }
    }
}