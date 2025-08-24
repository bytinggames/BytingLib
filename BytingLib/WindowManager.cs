using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    /// <summary>
    /// Provides the ability to toggle fullscreen and move window to another screen.
    /// </summary>
    public class WindowManager : IResolution
    {
        public ValueEvent<float> RenderScale { get; } = new(1f);
        public float Upscale => 1f / RenderScale.Value;

        public GameWindow Window { get; }
        private readonly bool realFullscreen;
        private readonly GraphicsDeviceManager graphics;
        private Rectangle windowRectBeforeFullscreen;
        public event Action<Int2>? OnResolutionChanged;
        /// <summary>Used for overriding the actual screen size to simulate a bigger screen or make screenshots at a higher resolution.</summary>
        public Int2? VirtualScreenSize { get; set; }
        /// <summary>Used for forcing a certain resolution when switching to window mode.</summary>
        public Int2? ForceWindowSize { get; set; }

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsZoomed(IntPtr hWnd); // used to check wether maximized window

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        /// <summary>Focuses on window. Makes it active.</summary>
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hwnd);

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

        public bool FullScreenPlus1Pixel { get; set; } = false;

        public WindowManager(bool realFullscreen, GameWindow window, GraphicsDeviceManager graphics)
        {
            this.realFullscreen = realFullscreen;
            this.Window = window;
            this.graphics = graphics;

            window.ClientSizeChanged += Window_ClientSizeChanged;
            RenderScale.OnChange += RenderScale_OnChange;
        }

        /// <summary>The render resolution</summary>
        public Int2 Resolution => new Int2(ResolutionX, ResolutionY);
        public int ResolutionX => Math.Max(1, (int)(WindowWidth / Upscale));
        public int ResolutionY => Math.Max(1, (int)(WindowHeight / Upscale));

        private Int2 WindowResolution => new Int2(WindowWidth, WindowHeight);
        private int WindowWidth => graphics.GraphicsDevice.PresentationParameters.Bounds.Width;
        private int WindowHeight => graphics.GraphicsDevice.PresentationParameters.Bounds.Height;


        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            // there's a bug, where the window size is 2px larger than the actual draw size.
            // if we would set the draw size to the window size, this process would repeat endlessly, making the window grow forever
            if (!Window.IsBorderless
                && graphics.PreferredBackBufferWidth == WindowWidth - 2
                && graphics.PreferredBackBufferHeight == WindowHeight - 8)
            {
                return;
            }

            if (graphics.PreferredBackBufferWidth != WindowWidth || graphics.PreferredBackBufferHeight != WindowHeight)
            {
                SetBackBufferSize(new Int2(WindowWidth, WindowHeight));

                OnResolutionChanged?.Invoke(Resolution);
            }
        }

        private void RenderScale_OnChange(float newUpscale)
        {
            OnResolutionChanged?.Invoke(Resolution);
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen())
            {
                graphics.PreferredBackBufferWidth = ForceWindowSize?.X ?? windowRectBeforeFullscreen.Width;
                graphics.PreferredBackBufferHeight = ForceWindowSize?.Y ?? windowRectBeforeFullscreen.Height;

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

                graphics.PreferredBackBufferWidth = GetTargetBackBufferWidth();
                graphics.PreferredBackBufferHeight = GetTargetBackBufferHeight();

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

            OnResolutionChanged?.Invoke(Resolution);
        }

        private int GetTargetBackBufferWidth()
        {
            return GetScreenWidth();
        }
        private int GetTargetBackBufferHeight()
        {
            return GetScreenHeight() + (FullScreenPlus1Pixel ? 1 : 0);
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (fullscreen != IsFullscreen())
            {
                ToggleFullscreen();
            }
        }

        /// <summary>This doesn't trigger going to fullscreen.</summary>
        public void SetFullScreenPlus1PixelMode(bool borderless)
        {
            if (FullScreenPlus1Pixel == borderless)
            {
                return;
            }
            FullScreenPlus1Pixel = borderless;
            if (IsFullscreen())
            {
                // update resolution
                graphics.PreferredBackBufferWidth = GetTargetBackBufferWidth();
                graphics.PreferredBackBufferHeight = GetTargetBackBufferHeight();
                graphics.ApplyChanges();
            }
        }

        public void SwapScreen()
        {
            Int2 rememberRes = Resolution;

            bool keepFullscreen = IsFullscreen();

            if (keepFullscreen && realFullscreen)
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
                // check if window is larger than screen
                Int2 newWindowRes = WindowResolution;
                if (WindowResolution.X > screenBounds.Width)
                {
                    newWindowRes.X = screenBounds.Width;
                }
                const int windowTabHeight = 32;
                if (WindowResolution.Y > screenBounds.Height - windowTabHeight)
                {
                    newWindowRes.Y = screenBounds.Height - windowTabHeight;
                }
                if (newWindowRes != WindowResolution)
                {
                    SetBackBufferSize(newWindowRes);
                }

                // center window on screen
                Window.Position = screenBounds.Center - (WindowResolution / 2).ToPoint();
            }

            if (keepFullscreen)
            {
                SetBackBufferSize(new Int2(screenBounds.Size));

                if (realFullscreen)
                {
                    graphics.ToggleFullScreen();
                }
            }
            if (rememberRes != Resolution)
            {
                OnResolutionChanged?.Invoke(Resolution);
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

        public int GetScreenRefreshRate()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.RefreshRate;
        }

        /// <summary>Only supported on Windows</summary>
        public void MaximizeWindow()
        {
#if WINDOWS
            ShowWindow(SW_MAXIMIZE);
#endif
        }

        /// <summary>Only supported on Windows</summary>
        public void MinimizeWindow()
        {
#if WINDOWS
            ShowWindow(SW_MINIMIZE);
#endif
        }

        private void ShowWindow(int action)
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            ShowWindow(hwnd, action);
            if (action == SW_MAXIMIZE)
            {
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
            }
#endif
        }

        private bool IsMaximized()
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            return IsZoomed(hwnd);
#endif

            return false;
        }

        public void SetBackBufferSize(Int2 backBufferSize)
        {
            graphics.PreferredBackBufferWidth = backBufferSize.X;
            graphics.PreferredBackBufferHeight = backBufferSize.Y;
            graphics.ApplyChanges();
        }

        public bool SetTopMost(bool setTopMost)
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            return SetWindowPos(hwnd, setTopMost ? HWND_TOPMOST : 0, 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOMOVE | SWP_NOSIZE);
#endif
            return false;
        }

        public void SetActive()
        {
#if WINDOWS
            IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, windowCaption);
            SetForegroundWindow(hwnd);
#endif
        }
    }
}
