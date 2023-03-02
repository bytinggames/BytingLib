using BytingLib.UI;

namespace BytingLib
{
    public class Animation : IDisposable
    {
        public Ref<Texture2D> Texture { get; }
        public Ref<AnimationData> Data { get; }

        private Rectangle[,]? sliceRects;

        public Animation(Ref<Texture2D> texture, Ref<AnimationData> data)
        {
            this.Texture = texture;
            this.Data = data;
        }

        public Animation(Ref<Texture2D> texture, string json)
        {
            this.Texture = texture;
            Data = new Ref<AnimationData>(new Pointer<AnimationData>(AnimationData.FromJson(json)), null);
        }

        public void Dispose()
        {
            Texture?.Dispose();
            Data?.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, string animationTagName, Anchor anchor, double ms)
        {
            Texture.Value.Draw(spriteBatch, anchor, null, Data.Value.GetSourceRectangle(ms, animationTagName));
        }

        public void Draw(SpriteBatch spriteBatch, int frameIndex, Anchor anchor)
        {
            Texture.Value.Draw(spriteBatch, anchor, null, Data.Value.GetSourceRectangle(frameIndex));
        }

        internal void DrawSliced(SpriteBatch spriteBatch, int frameIndex, Rect absoluteRect)
        {
            Rectangle frameRect = Data.Value.GetSourceRectangle(frameIndex)!.Value;
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

                    Texture.Value.Draw(spriteBatch, rect, null, sourceRect);
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

            var frame = Data.Value.frames!.First().Value;

            if (Data.Value.meta?.slices?.Length > 0)
            {
                sliceFace = Data.Value.meta!.slices![0].keys[0].bounds;
            }
            else
            {
                if (frame.rectangle.Width % 2 == 0
                    || frame.rectangle.Height % 2 == 0)
                {
                    throw new BytingException("Either add a slice to the texture that should get sliced or make sure width and height are odd, so the center pixel is automatically considered as the face slice");
                }

                sliceFace = new AnimationData._Rect()
                {
                    x = frame.rectangle.Width / 2,
                    y = frame.rectangle.Width / 2,
                    w = 1,
                    h = 1
                };
            }

            return sliceFace;
        }

        public Padding GetFacePadding()
        {
            var face = GetFaceSlice();
            var rect = Data.Value.frames!.First().Value.rectangle;
            return new Padding(face.x, face.y, rect.Width - face.x - face.w, rect.Height - face.y - face.h);
        }

        private Rectangle[,] GetSliceRects()
        {
            if (sliceRects != null)
                return sliceRects;

            sliceRects = new Rectangle[3, 3];

            AnimationData._Rect sliceFace = GetFaceSlice();

            var frame = Data.Value.frames!.First().Value;

            Int2[] sliceCoords = new Int2[4]
            {
                new Int2(0,0),
                new Int2(sliceFace.x, sliceFace.y),
                new Int2(sliceFace.x + sliceFace.w, sliceFace.y + sliceFace.h),
                new Int2(frame.rectangle.Width, frame.rectangle.Height)
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
