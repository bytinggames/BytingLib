namespace BytingLib.UI
{
    /// <summary>
    /// Used for updating Elements
    /// </summary>
    public class ElementInput
    {
        public IInputCanvas Input { get; }
        public Action<Element?> SetUpdateCatch { get; }
        public Element? FocusElement { get; set; }
        public Element? HoverElement { get; set; }
        public GameWindow Window { get; set; }

        private readonly Element hoverOutsideOfScissorRect = new Element();

        public ElementInput(IInputCanvas input, Action<Element?> setUpdateCatch, GameWindow window)
        {
            Input = input;
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
                return rect.CollidesWith(Input.MousePosition);
            }
            return false;
        }
    }
}
