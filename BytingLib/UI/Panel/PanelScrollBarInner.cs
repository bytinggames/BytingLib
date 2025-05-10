namespace BytingLib.UI
{
    internal class PanelScrollBarInner : Panel
    {
        private readonly PanelScroll scrollPanel;
        private readonly Button button;
        private float lastScroll = float.NaN;
        private float lastYDrag;
        private bool setDirtyLater;

        public PanelScrollBarInner(Color color, PanelScroll scrollPanel)
        {
            Color = color;
            this.scrollPanel = scrollPanel;
            button = new Button(() => { }, -1f, 1f, Vector2.Zero, null);
            Add(button);

            Padding = new Padding(0f);

            button.OnHoldBegin += OnHoldBegin;
            button.OnHoldSustain += OnHold;
        }
        
        protected override void UpdateTreeModifyRect(Rect rect)
        {
            base.UpdateTreeModifyRect(rect);

            UpdateInner(true);
        }

        private void OnHoldBegin(ElementInput input)
        {
            lastYDrag = input.Input.MousePosition.Y;
        }

        private void OnHold(ElementInput input)
        {
            float dragRelative = input.Input.MousePosition.Y - lastYDrag;
            lastYDrag = input.Input.MousePosition.Y;

            if (dragRelative != 0f)
            {
                float barSpace = AbsoluteRect.Height - button.Height;
                float scrollSpace = scrollPanel.GetMaxScrollY();
                
                scrollPanel.Scroll += dragRelative * scrollSpace / barSpace;

                UpdateInner();
            }
        }

        protected override void UpdateSelf(ElementInput input)
        {
            base.UpdateSelf(input);

            UpdateInner();
        }

        private void UpdateInner(bool updateTree = false)
        {
            if (lastScroll != scrollPanel.Scroll)
            {
                lastScroll = scrollPanel.Scroll;
                float maxScroll = scrollPanel.GetMaxScrollY();
                float scroll = scrollPanel.Scroll;
                float totalSpace = scrollPanel.GetTotalHeight();
                float shownHeight = scrollPanel.GetShownHeight();

                float scrollButtonFraction = shownHeight / totalSpace;
                float yFraction = scroll / maxScroll;

                var rect = AbsoluteRect;
                button.Height = rect.Size.Y * scrollButtonFraction;
                if (Padding == null)
                {
                    Padding = new(0f);
                }
                Padding.Top = (rect.Height - button.Height) * yFraction;

                if (updateTree)
                {
                    setDirtyLater = true;
                }
                else
                {
                    SetDirty();
                }
            }
            else if (setDirtyLater)
            {
                setDirtyLater = false;
                SetDirty();
            }
        }
    }
}
