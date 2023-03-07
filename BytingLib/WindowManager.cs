using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    /// <summary>
    /// Provides the ability to toggle fullscreen and move window to another screen.
    /// </summary>
    public class WindowManager : IResolution
    {
        private readonly bool realFullscreen;
        public GameWindow Window { get; }
        private readonly GraphicsDeviceManager graphics;

        private Rectangle windowRectBeforeFullscreen;

        public event Action<Int2>? OnResolutionChanged;

        private const int SW_MAXIMIZE = 3;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#if WINDOWS
        string windowCaption = Process.GetCurrentProcess().ProcessName;
#endif

        public WindowManager(bool realFullscreen, GameWindow window, GraphicsDeviceManager graphics)
        {
            this.realFullscreen = realFullscreen;
            this.Window = window;
            this.graphics = graphics;

            window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        public Int2 Resolution => new Int2(GetViewportWidth(), GetViewportHeight());

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            OnResolutionChanged?.Invoke(Resolution);
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen())
            {
                graphics.PreferredBackBufferWidth = windowRectBeforeFullscreen.Width;
                graphics.PreferredBackBufferHeight = windowRectBeforeFullscreen.Height;

                if (realFullscreen)
                    graphics.ToggleFullScreen();
                else
                    Window.IsBorderless = false;

                // set position to last window position, or if that is outside of the current screen bounds, simply center the window on the current screen
                Rectangle bounds = GraphicsAdapter.GetCurrentDisplayBounds(Window.Handle);
                if (bounds.Contains(windowRectBeforeFullscreen.Location))
                    Window.Position = windowRectBeforeFullscreen.Location;
                else
                    Window.Position = bounds.Center - new Point(windowRectBeforeFullscreen.Width / 2, windowRectBeforeFullscreen.Height / 2);

                graphics.ApplyChanges();
            }
            else
            {
                windowRectBeforeFullscreen = Window.ClientBounds;

                if (!realFullscreen)
                {
                    var bounds = GraphicsAdapter.GetCurrentDisplayBounds(Window.Handle);
                    Window.IsBorderless = true;
                    Window.Position = new Point(bounds.X, bounds.Y);
                }

                graphics.PreferredBackBufferWidth = GetScreenWidth();
                graphics.PreferredBackBufferHeight = GetScreenHeight();

                if (!realFullscreen)
                    graphics.ApplyChanges();
                else
                {
#if !WINDOWS
                    // this is required on linux, otherwise the screen would just turn black and freeze
                    graphics.ApplyChanges();
#endif
                    graphics.ToggleFullScreen();
                }
            }


            OnResolutionChanged?.Invoke(Resolution);
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
            bool keepFullscreen = realFullscreen && IsFullscreen();

            if (keepFullscreen)
                graphics.ToggleFullScreen();

            int screenIndex = GraphicsAdapter.GetCurrentDisplayIndex(Window.Handle);
            int screenCount = GraphicsAdapter.GetDisplayCount();
            screenIndex = (screenIndex + 1) % screenCount;
            Rectangle screenBounds = GraphicsAdapter.GetDisplayBounds(screenIndex);

            if (keepFullscreen)
            {
#if !WINDOWS
                // somehow this is required on my linux laptop
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 600;
                graphics.ApplyChanges();
#endif
                Window.Position = screenBounds.Location;
#if !WINDOWS
                // somehow this is required on my linux laptop
                graphics.PreferredBackBufferWidth = GetScreenWidth();
                graphics.PreferredBackBufferHeight = GetScreenHeight();
                graphics.ApplyChanges();
#endif
            }
            else
                Window.Position = screenBounds.Center - (Resolution / 2).ToPoint();

            if (keepFullscreen)
                graphics.ToggleFullScreen();
        }

        /// <summary>
        /// don't call if in true fullscreen
        /// </summary>
        private void MoveWindow(int byX, int byY)
        {
            Window.Position = new Point(Window.Position.X + byX, Window.Position.Y + byY);
        }

        public bool IsFullscreen()
        {
            if (realFullscreen)
                return graphics.IsFullScreen;
            else
                return Window.IsBorderless;
        }

        private static int GetScreenHeight()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        private static int GetScreenWidth()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        }


        /// <summary>Only supported on Windows</summary>
        public void MaximizeWindow()
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            ShowWindow(hwnd, SW_MAXIMIZE);
#endif
        }
    }
}
