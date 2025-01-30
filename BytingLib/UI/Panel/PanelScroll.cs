namespace BytingLib.UI
{
    public class PanelScroll : PanelCut
    {
        public float ScrollSpeed = 128f;

        public PanelScroll(float width = -1f, float height = -1f, Color? color = null, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, color, anchor, padding)
        {
        }

        public override void Update(ElementInput input)
        {
            if (input.Input.Scroll != 0)
            {
                int scroll = Math.Sign(input.Input.Scroll);
                float y;
                if (Padding == null)
                {
                    y = ScrollSpeed * scroll;
                    if (scroll < 0f)
                    {
                        Padding = new Padding(0f, ScrollSpeed * scroll, 0f, 0f);
                    }
                }
                else
                {
                    y = Padding.Top + ScrollSpeed * scroll;
                }

                if (y > 0)
                {
                    y = 0f;
                }
                else
                {
                    float maxY = Children.Max(f => f.GetSizeTopToBottom(1, new Vector2(float.PositiveInfinity, float.PositiveInfinity)));
                    maxY -= AbsoluteRect.Size.Y;
                    if (-y > maxY)
                    {
                        y = -maxY;
                        if (y > 0f)
                        {
                            y = 0f;
                        }
                    }
                }
                if (Padding == null)
                {
                    if (y != 0f)
                    {
                        Padding = new Padding(0f, y, 0f, 0f);
                        SetDirty();
                    }
                }
                else
                {
                    if (y != Padding.Top)
                    {
                        Padding.Top = y;
                        SetDirty();
                    }
                }
            }

            base.Update(input);
        }
    }
}
