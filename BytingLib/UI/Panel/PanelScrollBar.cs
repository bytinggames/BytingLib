namespace BytingLib.UI
{
    internal class PanelScrollBar : PanelStack
    {
        private readonly PanelScroll scrollPanel;

        public ScrollArrowButton ButtonUp { get; }
        public ScrollArrowButton ButtonDown { get; }

        public PanelScrollBar(float width, float height, Color backgroundColor, Color foregroundColor, PanelScroll scrollPanel)
        {
            this.scrollPanel = scrollPanel;
         
            Width = width;
            Height = height;
            Color = backgroundColor;

            Add(ButtonUp = new ScrollArrowButton(true, scrollPanel.ScrollUp, width, width));
            Add(new PanelScrollBarInner(foregroundColor, scrollPanel));
            Add(ButtonDown = new ScrollArrowButton(false, scrollPanel.ScrollDown, width, width));
        }

        protected override void UpdateTreeInner(Rect parentRect)
        {
            Visible = scrollPanel.GetMaxScrollY() > 0f;
            
            base.UpdateTreeInner(parentRect);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (Color != null)
            {
                AbsoluteRect.Draw(spriteBatch, Color.Value);
            }
        }
    }
}
