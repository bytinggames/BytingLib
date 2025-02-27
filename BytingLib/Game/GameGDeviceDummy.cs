namespace BytingLib
{
    internal class GameGDeviceDummy : Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; }

        public GameGDeviceDummy()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this); // this is required for the graphics device
        }

        protected override void Dispose(bool disposing)
        {
            GraphicsDeviceManager.Dispose();

            base.Dispose(disposing);
        }
    }
}