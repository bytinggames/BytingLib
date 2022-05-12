using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    /// <summary>
    /// Provides the ability to toggle fullscreen and move window to another screen.
    /// </summary>
    public class WindowManager : IGetResolution
    {
        private readonly bool realFullscreen;
        private readonly GameWindow window;
        private readonly GraphicsDeviceManager graphics;

        private Int2 windowSizeBeforeFullscreen;

        public event Action<Int2>? OnResolutionChanged;

        public WindowManager(bool realFullscreen, GameWindow window, GraphicsDeviceManager graphics)
        {
            this.realFullscreen = realFullscreen;
            this.window = window;
            this.graphics = graphics;

            window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            OnResolutionChanged?.Invoke(GetResolution());
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen())
            {
                if (realFullscreen)
                    graphics.ToggleFullScreen();
                else
                    window.IsBorderless = false;
                graphics.PreferredBackBufferWidth = windowSizeBeforeFullscreen.X;
                graphics.PreferredBackBufferHeight = windowSizeBeforeFullscreen.Y;
                
                window.Position = new Point(
                    (GetScreenWidth() - windowSizeBeforeFullscreen.X) / 2,
                    (GetScreenHeight() - windowSizeBeforeFullscreen.Y) / 2);
                graphics.ApplyChanges();
            }
            else
            {
                windowSizeBeforeFullscreen = new Int2(
                    GetViewportWidth(),
                    GetViewportHeight());

                if (!realFullscreen)
                {
                    window.IsBorderless = true;
                    window.Position = new Point(0, 0);
                }

                graphics.PreferredBackBufferWidth = GetScreenWidth();
                graphics.PreferredBackBufferHeight = GetScreenHeight();
                graphics.ApplyChanges();

                if (realFullscreen)
                    graphics.ToggleFullScreen();
            }
        }

        private int GetViewportWidth()
        {
            return graphics.GraphicsDevice.Viewport.Width;
        }

        private int GetViewportHeight()
        {
            return graphics.GraphicsDevice.Viewport.Height;
        }

        public void SwapScreen()
        {
            bool isFullScreen = realFullscreen && IsFullscreen();

            if (isFullScreen)
                graphics.ToggleFullScreen();

            if (window.Position.X < GetScreenWidth())
                MoveWindow(GetScreenWidth(), 0);
            else
                MoveWindow(-GetScreenWidth(), 0);

            if (isFullScreen)
                graphics.ToggleFullScreen();
        }

        /// <summary>
        /// don't call if in true fullscreen
        /// </summary>
        private void MoveWindow(int byX, int byY)
        {
            window.Position = new Point(window.Position.X + byX, window.Position.Y + byY);
        }

        public bool IsFullscreen()
        {
            if (realFullscreen)
                return graphics.IsFullScreen;
            else
                return window.IsBorderless;
        }

        private static int GetScreenHeight()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        private static int GetScreenWidth()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        }

        public Int2 GetResolution()
        {
            return new Int2(GetViewportWidth(), GetViewportHeight());
        }
    }
}
