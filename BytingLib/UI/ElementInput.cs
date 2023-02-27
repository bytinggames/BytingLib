namespace BytingLib.UI
{
    /// <summary>
    /// Used for updating Elements
    /// </summary>
    public class ElementInput
    {
        public MouseInput Mouse { get; }
        public Action<Element?> SetUpdateCatch { get; }

        public ElementInput(MouseInput mouse, Action<Element?> setUpdateCatch)
        {
            Mouse = mouse;
            SetUpdateCatch = setUpdateCatch;
        }
    }
}
