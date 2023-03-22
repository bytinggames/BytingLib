using BytingLib.UI;

namespace BytingLib
{
    public class Animation : IDisposable
    {
        private readonly Ref<Texture2D> textureRef;
        public Texture2D Texture => textureRef.Value;
        public AnimationData Data { get; }

        private Rectangle[,]? sliceRects;

        public Animation(Ref<Texture2D> texture, string json)
        {
            textureRef = texture;
            Data = AnimationData.FromJson(json);
        }

        public void Dispose()
        {
            textureRef.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, string animationTagName, Anchor anchor, double ms)
        {
            Texture.Draw(spriteBatch, anchor, null, Data.GetSourceRectangle(ms, animationTagName));
        }

        public void Draw(SpriteBatch spriteBatch, int frameIndex, Anchor anchor)
        {
            Texture.Draw(spriteBatch, anchor, null, Data.GetSourceRectangle(frameIndex));
        }

        internal void DrawSliced(SpriteBatch spriteBatch, int frameIndex, Rect absoluteRect)
        {
            Rectangle frameRect = Data.GetSourceRectangle(frameIndex)!.Value;
            Rectangle sourceRect;
            Rect rect;

            Rectangle[,] sliceRects = GetSliceRects();
            Vector2[] worldCoords = new Vector2[4]
            {
                absoluteRect.Pos,
                absoluteRect.Pos + sliceRects[1,1].Location.ToVector2(),
                absoluteRect.BottomRight - sliceRects[2,2].Location.ToVector2() + Vector2.One, // not sure why +(1,1) but fixes the issue of stretched sides at the right and bottom
                absoluteRect.BottomRight
            };

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    sourceRect = sliceRects[x, y];
                    sourceRect.X += frameRect.X;
                    sourceRect.Y += frameRect.Y;
                    rect = GetWorldRect(x, y);

                    Texture.Draw(spriteBatch, rect, null, sourceRect);
                }
            }

            Rect GetWorldRect(int xPatch, int yPatch)
            {
                float x = worldCoords[xPatch].X;
                float y = worldCoords[yPatch].Y;
                return new Rect(x, y, worldCoords[xPatch + 1].X - x, worldCoords[yPatch + 1].Y - y);
            }
        }

        public AnimationData._Rect GetFaceSlice()
        {
            AnimationData._Rect sliceFace;

            var frame = Data.frames!.First();

            if (Data.meta?.slices?.Length > 0)
            {
                sliceFace = Data.meta!.slices![0].keys[0].bounds;
            }
            else
            {
                if (frame.Value.rectangle.Width % 2 == 0
                    || frame.Value.rectangle.Height % 2 == 0)
                {
                    throw new BytingException("Either add a slice to the texture that should get sliced or make sure width and height are odd, so the center pixel is automatically considered as the face slice");
                }

                sliceFace = new AnimationData._Rect()
                {
                    x = frame.Value.rectangle.Width / 2,
                    y = frame.Value.rectangle.Width / 2,
                    w = 1,
                    h = 1
                };
            }

            return sliceFace;
        }

        public Padding GetFacePadding()
        {
            var face = GetFaceSlice();
            var rect = Data.frames!.First().Value.rectangle;
            return new Padding(face.x, face.y, rect.Width - face.x - face.w, rect.Height - face.y - face.h);
        }

        private Rectangle[,] GetSliceRects()
        {
            if (sliceRects != null)
                return sliceRects;

            sliceRects = new Rectangle[3, 3];

            AnimationData._Rect sliceFace = GetFaceSlice();

            var frame = Data.frames!.First();

            Int2[] sliceCoords = new Int2[4]
            {
                new Int2(0,0),
                new Int2(sliceFace.x, sliceFace.y),
                new Int2(sliceFace.x + sliceFace.w, sliceFace.y + sliceFace.h),
                new Int2(frame.Value.rectangle.Width, frame.Value.rectangle.Height)
            };

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    sliceRects[x,y] = GetSliceRect(x, y);

            Rectangle GetSliceRect(int xPatch, int yPatch)
            {
                int x = sliceCoords[xPatch].X;
                int y = sliceCoords[yPatch].Y;
                return new Rectangle(x, y, sliceCoords[xPatch + 1].X - x, sliceCoords[yPatch + 1].Y - y);
            }

            return sliceRects;
        }
    }
}
