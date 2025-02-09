
namespace BytingLib
{
    public class BindsCanvas
    {
        #region >InputCanvas

        public InputVector2 MousePosition { get; set; } = new Vector2Mouse();
        public InputBool Click { get; set; } = MouseButton.Left;
        public InputInt Scroll { get; set; } = new IntMouseWheel();

        #endregion
    }
}
