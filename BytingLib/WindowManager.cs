using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    /// <summary>
    /// Provides the ability to toggle fullscreen and move window to another screen.
    /// </summary>
    public class WindowManager : IResolution
    {
        public GameWindow Window { get; }
        public Rect Rect { get; private set; }
        private readonly bool realFullscreen;
        private readonly GraphicsDeviceManager graphics;
        private Rectangle windowRectBeforeFullscreen;
        public event Action<Int2>? OnResolutionChanged;
        /// <summary>Used for overriding the actual screen size to simulate a bigger screen or make screenshots at a higher resolution.</summary>
        public Int2? VirtualScreenSize { get; set; }

        private const int SW_MAXIMIZE = 3;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


#if WINDOWS
        string windowCaption = Process.GetCurrentProcess().ProcessName;
#endif

        public WindowManager(bool realFullscreen, GameWindow window, GraphicsDeviceManager graphics)
        {
            this.realFullscreen = realFullscreen;
            this.Window = window;
            this.graphics = graphics;

            window.ClientSizeChanged += Window_ClientSizeChanged;
            Rect = new Rect(0, 0, ResolutionX, ResolutionY);
        }

        public Int2 Resolution => new Int2(GetViewportWidth(), GetViewportHeight());
        public int ResolutionX => GetViewportWidth();
        public int ResolutionY => GetViewportHeight();


        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            OnResolutionChanged?.Invoke(Resolution);

            Rect.Width = ResolutionX;
            Rect.Height = ResolutionY;
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen())
            {
                graphics.PreferredBackBufferWidth = windowRectBeforeFullscreen.Width;
                graphics.PreferredBackBufferHeight = windowRectBeforeFullscreen.Height;

                if (realFullscreen)
                {
                    graphics.ToggleFullScreen();
                }
                else
                {
                    Window.IsBorderless = false;
                }

                // set position to last window position, or if that is outside of the current screen bounds, simply center the window on the current screen
                Rectangle bounds = GraphicsAdapter.GetCurrentDisplayBounds(Window.Handle);
                if (bounds.Contains(windowRectBeforeFullscreen.Location))
                {
                    Window.Position = windowRectBeforeFullscreen.Location;
                }
                else
                {
                    Window.Position = bounds.Center - new Point(windowRectBeforeFullscreen.Width / 2, windowRectBeforeFullscreen.Height / 2);
                }

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

                if (realFullscreen)
                {
#if !WINDOWS
                    // this is required on linux, otherwise the screen would just turn black and freeze
                    graphics.ApplyChanges();
#endif
                    graphics.ToggleFullScreen();
                }
                else
                {
                    graphics.ApplyChanges();
                }
            }


            Window_ClientSizeChanged(null, EventArgs.Empty);
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (fullscreen != IsFullscreen())
            {
                ToggleFullscreen();
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
            bool keepFullscreen = realFullscreen && IsFullscreen();

            if (keepFullscreen)
            {
                graphics.ToggleFullScreen();
            }

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
            {
                Window.Position = screenBounds.Center - (Resolution / 2).ToPoint();
            }

            if (keepFullscreen)
            {
                graphics.ToggleFullScreen();
            }
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
            {
                return graphics.IsFullScreen;
            }
            else
            {
                return Window.IsBorderless;
            }
        }

        private int GetScreenWidth()
        {
            if (VirtualScreenSize == null)
            {
                return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            }
            else
            {
                return VirtualScreenSize.Value.X;
            }
        }

        private int GetScreenHeight()
        {
            if (VirtualScreenSize == null)
            {
                return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                return VirtualScreenSize.Value.Y;
            }
        }

        /// <summary>Only supported on Windows</summary>
        public void MaximizeWindow()
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            ShowWindow(hwnd, SW_MAXIMIZE);
            RECT r = new();
            GetClientRect(hwnd, out r);

            // make sure graphics.PreferredBackBufferWidth and Height are updated correctly
            // also apply graphics changes now, because this alters the window position a bit if the window is maximized.
            // if we do this now, we can fix the position offset right away

            graphics.PreferredBackBufferWidth = r.Right - r.Left;
            graphics.PreferredBackBufferHeight = r.Bottom - r.Top;

            var rememberPosition = Window.Position;

            graphics.ApplyChanges();

            // fix position offset that may occur because of graphics.ApplyChanges() here
            Window.Position = rememberPosition;
#endif
        }


        public void SetWindowResolution(Int2 resolution)
        {
            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;
            graphics.ApplyChanges();
        }
    }
}
