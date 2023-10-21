namespace BytingLib
{
    public class MessageBox : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private readonly string text;
        private readonly string fontAssetName;
        private SpriteFont font;

        const int Padding = 16;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MessageBox(string text, string fontAssetName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            int lineLength = 0;
            for (int i = 0; i < text.Length; i++, lineLength++)
            {
                if (text[i] == '\n')
                {
                    lineLength = 0;
                }
                else if (lineLength >= 200)
                {
                    text = text.Remove(i) + "\n        " + text.Substring(i);
                    lineLength = 0;
                }
            }


            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            this.text = text;
            this.fontAssetName = fontAssetName;

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.font = Content.Load<SpriteFont>(fontAssetName);

            Vector2 size = font.MeasureString(text);
            size += new Vector2(Padding * 2);
            graphics.PreferredBackBufferWidth = (int)size.X;
            graphics.PreferredBackBufferHeight = (int)size.Y;

            graphics.ApplyChanges();

            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(200,200,200));

            spriteBatch.Begin();

            spriteBatch.DrawString(font, text, new Vector2(Padding), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
