using BytingLib;
using BytingLib.Intro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace BytingIntroSandbox
{
#pragma warning disable CS0612 // Type or member is obsolete
    public class Game1 : _BaseGame
#pragma warning restore CS0612 // Type or member is obsolete
    {
        static readonly bool forWebsite = true;
        const int forWebsiteScale = 4; // 4 to convert to svg

        protected readonly GameSpeed updateSpeed, drawSpeed;
        protected IStuffDisposable gameStuff;
        protected KeyInput keys;
        protected MouseInput mouse;
        protected GamePadInput gamePad;
        protected bool triggerRestart;
        protected WindowManager windowManager;
        private BytingIntro intro;

        public Game1()
        {
            IsMouseVisible = true;
            updateSpeed = new GameSpeed(TargetElapsedTime);
            drawSpeed = new GameSpeed(TargetElapsedTime);
            InactiveBlendColor = null;

            if (forWebsite)
            {
                graphics.PreferredBackBufferWidth = 600 * forWebsiteScale;
                graphics.PreferredBackBufferHeight = 340 * forWebsiteScale;
            }
        }

        protected override void MyInitialize()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            gameStuff = _BaseGameFactory.CreateDefaultGame(this, graphics, "input", out keys, out mouse, out gamePad, out windowManager, false); disposables.Add(gameStuff);
#pragma warning restore CS0612 // Type or member is obsolete

            intro = new BytingIntro(mouse, keys);

            if (!forWebsite)
            {
                IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "BytingIntroSandbox");
                ShowWindow(hwnd, SW_MAXIMIZE);
            }
        }

        protected override void UpdateActive(GameTime gameTime)
        {
            updateSpeed.OnRefresh(gameTime);

            gameStuff.ForEach<IUpdate>(f => f.Update());

            intro.Update();

            if (keys.R.Pressed)
            {
                intro.Dispose();
                intro = new BytingIntro(mouse, keys);
            }
        }
        protected override void DrawActive(GameTime gameTime)
        {
            drawSpeed.OnRefresh(gameTime);

            var tex = intro.DrawOnMyOwn(spriteBatch, new Int2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            if (keys.Enter.Pressed)
            {
                tex = ExportPng(tex);
            }

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            tex.Draw(spriteBatch, Vector2.Zero);

            spriteBatch.End();
        }

        private static Texture2D ExportPng(Texture2D tex)
        {
            Color[] colors = tex.ToColor();

            // correct premultiplied alpha
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].R = 0;
                colors[i].G = 0;
                colors[i].B = 0;
            }

            tex = colors.ToTexture(tex.Width, tex.GraphicsDevice);

            using (var trimmed = tex.GetTrimmed())
            {
                trimmed.SaveAsPng("logo-export.png");
            }
            return tex;
        }

        protected override void UnloadContent()
        {
            intro.Dispose();

            base.UnloadContent();
        }

        private const int SW_MAXIMIZE = 3;
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }
}
