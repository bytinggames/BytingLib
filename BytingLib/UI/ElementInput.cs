namespace BytingLib.UI
{
    /// <summary>
    /// Used for updating Elements
    /// </summary>
    public class ElementInput
    {
        public MouseInput Mouse { get; }
        public KeyInput Keys { get; }
        public Action<Element?> SetUpdateCatch { get; }
        public Element? FocusElement { get; set; }
        public Element? HoverElement { get; set; }
        public GameWindow Window { get; set; }

        private readonly Element hoverOutsideOfScissorRect = new Element();

        public ElementInput(MouseInput mouse, KeyInput keys, Action<Element?> setUpdateCatch, GameWindow window)
        {
            Mouse = mouse;
            Keys = keys;
            SetUpdateCatch = setUpdateCatch;
            Window = window;
        }

        public void DoWhileHoverOutsideOfScissorRect(Action actionWhileHoverDisabled)
        {
            var rememberHoverElement = HoverElement;
            if (rememberHoverElement == null)
            {
                HoverElement = hoverOutsideOfScissorRect;
            }
            actionWhileHoverDisabled();

            if (rememberHoverElement == null)
            {
                HoverElement = rememberHoverElement;
            }
        }

        public bool CanHover(Rect rect, Element element)
        {
            if (HoverElement == null || element == HoverElement)
            {
                return rect.CollidesWith(Mouse.Position);
            }
            return false;
        }
    }
}
