using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    /// <summary>
    /// Provides the ability to toggle fullscreen and move window to another screen.
    /// </summary>
    public class WindowManager : IGetResolution
    {
        private readonly bool realFullscreen;
        public GameWindow Window { get; }
        private readonly GraphicsDeviceManager graphics;

        private Int2 windowSizeBeforeFullscreen;

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

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            OnResolutionChanged?.Invoke(GetResolution());
        }

        public void ToggleFullscreen()
        {
            if (IsFullscreen())
            {
                graphics.PreferredBackBufferWidth = windowSizeBeforeFullscreen.X;
                graphics.PreferredBackBufferHeight = windowSizeBeforeFullscreen.Y;

                if (realFullscreen)
                    graphics.ToggleFullScreen();
                else
                    Window.IsBorderless = false;

                Window.Position = new Point(
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
                    Window.IsBorderless = true;
                    Window.Position = new Point(0, 0);
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

            if (Window.Position.X < GetScreenWidth())
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

        public Int2 GetResolution()
        {
            return new Int2(GetViewportWidth(), GetViewportHeight());
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
