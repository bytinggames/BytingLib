namespace BytingLib
{
    public interface IInputCanvas
    {
        public InputVector2State MousePosition { get; }
        public InputBoolState Click { get; }
        public InputIntState Scroll { get; }
        public InputUpdater Updater { get; }
        public InputVector2State Navigate { get; }
        public InputVector2State NavigateWithLetters { get; }
        public InputBoolState Enter { get; }
        void SetMousePosition(Vector2 position);
    }
}
