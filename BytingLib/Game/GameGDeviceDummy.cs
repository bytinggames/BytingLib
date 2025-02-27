namespace BytingLib
{
    internal class GameGDeviceDummy : Game
    {
        public GraphicsDeviceManager graphics { get; }

        public GameGDeviceDummy()
        {
            graphics = new GraphicsDeviceManager(this); // this is required for the graphics device
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Dispose(bool disposing)
        {
            graphics.Dispose();

            base.Dispose(disposing);
        }
    }
}