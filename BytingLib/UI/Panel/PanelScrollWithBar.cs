namespace BytingLib.UI
{
    public class PanelScrollWithBar : PanelStack
    {
        public PanelScroll PanelScroll { get; }
        internal PanelScrollBar PanelScrollBar { get; }

        public PanelScrollWithBar(Color barColorBack, Color barColorFront, float scrollBarWidth = 64f, float width = -1f, float height = -1f, Color? panelColor = null, float gap = 0f)
            : base(gap, false, null, Vector2.Zero, null)
        {
            Width = width;
            Height = height;
            Add(PanelScroll = new PanelScroll(width <= 0 ? width : Math.Max(1, width - scrollBarWidth), height, panelColor, Vector2.Zero, null));
            Add(PanelScrollBar = new PanelScrollBar(scrollBarWidth, height, barColorBack, barColorFront, PanelScroll));
        }

        public void SetButtonHoverStyle(Style style)
        {
            PanelScrollBar.ButtonDown.HoverStyle = style;
            PanelScrollBar.ButtonUp.HoverStyle = style;
        }
    }
}
