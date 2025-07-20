namespace BytingLib.UI
{
    /// <summary>
    /// Used for updating Elements
    /// </summary>
    public class ElementInput
    {
        public IInputCanvas Input { get; }
        public Action<Element?> SetUpdateCatch { get; }
        public Action<Element> UnsetUpdateCatch { get; }
        public Element? FocusElement { get; set; }
        public Element? NavigateElement { get; set; }
        public Element? HoverElement { get; set; }
        public GameWindow Window { get; set; }
        public GameSpeed UpdateSpeed { get; set; }

        private readonly Element hoverOutsideOfScissorRect = new Element();

        public ElementInput(IInputCanvas input, Action<Element?> setUpdateCatch, Action<Element> unsetUpdateCatch, GameWindow window, GameSpeed updateSpeed)
        {
            Input = input;
            SetUpdateCatch = setUpdateCatch;
            UnsetUpdateCatch = unsetUpdateCatch;
            Window = window;
            UpdateSpeed = updateSpeed;
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

        public bool CanHover(IShape shape, Element element)
        {
            if (NavigateElement == element)
            {
                return true;
            }

            if (HoverElement == null || element == HoverElement)
            {
                return shape.CollidesWith(Input.MousePosition);
            }
            return false;
        }
    }
}
