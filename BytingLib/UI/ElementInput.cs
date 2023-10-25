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

        public ElementInput(MouseInput mouse, KeyInput keys, Action<Element?> setUpdateCatch, GameWindow window)
        {
            Mouse = mouse;
            Keys = keys;
            SetUpdateCatch = setUpdateCatch;
            Window = window;
        }
    }
}
