namespace BytingLib
{
    public interface IInputCanvas
    {
        public Vector2Output MousePosition { get; }
        public BoolOutput Click { get; }
        public IntOutput Scroll { get; }
        public InputUpdater Updater { get; }
        void SetMousePosition(Vector2 position);
    }
}
