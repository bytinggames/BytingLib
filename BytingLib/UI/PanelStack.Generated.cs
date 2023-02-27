
namespace BytingLib.UI
{
	public partial class PanelStack
	{

        private void DrawVertical(SpriteBatch spriteBatch, Vector2 pos, Vector2 contentSize, bool anyUnknownHeight, Rect rect)
        {
            float nullHeight = 0f;
            float maxWidthPercentage = Children.Max(f => -MathF.Min(0, f.Width));
            if (anyUnknownHeight)
            {
                float autoHeightSum = Children.Sum(f => -MathF.Min(0, f.Height));
                float fixedHeight = GetFixedHeight();
                float remainingHeight = contentSize.Y - fixedHeight;
                nullHeight = remainingHeight / autoHeightSum;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                float height = c.Height >= 0 ? c.Height : -c.Height * nullHeight;
                float width = c.Width >= 0 ? c.Width : -c.Width * rect.Width / maxWidthPercentage;
                float remainingWidth = rect.Width - width;
                c.Draw(spriteBatch, new Rect(pos + new Vector2( remainingWidth * c.Anchor.X , 0f), new Vector2(width, height)));
                pos.Y += height + Gap;
            }
        }

        private float GetFixedHeight()
        {
            return Children.Sum(f => MathF.Max(0f, f.Height)) + Gap * (Children.Count - 1);
        }

        private Vector2 GetContentSizeVertical(out bool anyUnknownHeight)
        {
            float width = Children.Count == 0 ? 0 : Children.Max(f => f.GetWidth());
            float height;

            if (Children.Any(f => f.Height < 0))
            {
                // take 100% height
                height = GetHeight();
                anyUnknownHeight = true;
            }
            else
            {
                height = GetFixedHeight();
                anyUnknownHeight = false;
            }

            return new Vector2(width, height);
        }

        private void DrawHorizontal(SpriteBatch spriteBatch, Vector2 pos, Vector2 contentSize, bool anyUnknownWidth, Rect rect)
        {
            float nullWidth = 0f;
            float maxHeightPercentage = Children.Max(f => -MathF.Min(0, f.Height));
            if (anyUnknownWidth)
            {
                float autoWidthSum = Children.Sum(f => -MathF.Min(0, f.Width));
                float fixedWidth = GetFixedWidth();
                float remainingWidth = contentSize.X - fixedWidth;
                nullWidth = remainingWidth / autoWidthSum;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                float width = c.Width >= 0 ? c.Width : -c.Width * nullWidth;
                float height = c.Height >= 0 ? c.Height : -c.Height * rect.Height / maxHeightPercentage;
                float remainingHeight = rect.Height - height;
                c.Draw(spriteBatch, new Rect(pos + new Vector2(0f , remainingHeight * c.Anchor.Y ), new Vector2(width, height)));
                pos.X += width + Gap;
            }
        }

        private float GetFixedWidth()
        {
            return Children.Sum(f => MathF.Max(0f, f.Width)) + Gap * (Children.Count - 1);
        }

        private Vector2 GetContentSizeHorizontal(out bool anyUnknownWidth)
        {
            float height = Children.Count == 0 ? 0 : Children.Max(f => f.GetHeight());
            float width;

            if (Children.Any(f => f.Width < 0))
            {
                // take 100% width
                width = GetWidth();
                anyUnknownWidth = true;
            }
            else
            {
                width = GetFixedWidth();
                anyUnknownWidth = false;
            }

            return new Vector2(width, height);
        }
	}
}