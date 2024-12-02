namespace BytingLib
{
    public class TextureFrame
    {
        private float frame = 0;
        public float Frame
        {
            get => frame;
            set
            {
                frame = value % TotalFrames;
                if (frame < 0)
                {
                    frame += TotalFrames;
                }
            }
        }
        public int Columns { get; }
        public int Rows { get; }
        public int TotalFrames { get; }
        private Rectangle frameRect;
        private int frameRectFrameNumber = -1;
        private readonly Ref<Texture2D> texture;

        public TextureFrame(Ref<Texture2D> texture, int columns, int totalFrames)
        {
            this.texture = texture;
            Columns = columns;
            TotalFrames = totalFrames;
            Rows = GetRows(TotalFrames, Columns);
            frameRect = new();
        }

        private static int GetRows(int totalFrames, int columns)
        {
            return (int)MathF.Ceiling((float)totalFrames / columns);
        }

        public Rectangle GetRectangle()
        {
            if (frameRectFrameNumber != frame)
            {
                int iFrame = (int)frame;
                frameRectFrameNumber = iFrame;
                int x = iFrame % Columns;
                int y = iFrame / Columns;
                int w = texture.Value.Width / Columns;
                int h = texture.Value.Height / Rows;
                frameRect.X = x * w;
                frameRect.Y = y * h;
                frameRect.Width = w;
                frameRect.Height = h;
            }

            return frameRect;
        }
    }
}
