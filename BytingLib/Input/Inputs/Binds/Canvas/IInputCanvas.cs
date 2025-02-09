namespace BytingLib
{
    public interface IInputCanvas
    {
        public InputVector2State MousePosition { get; }
        public InputBoolState Click { get; }
        public InputIntState Scroll { get; }
        public InputUpdater Updater { get; }
        void SetMousePosition(Vector2 position);
    }
}
