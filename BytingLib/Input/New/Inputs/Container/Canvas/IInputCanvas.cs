namespace BytingLib
{
    public interface IInputCanvas
    {
        public Vector2Output MousePosition { get; }
        public BoolOutput LeftClick { get; }
        public IntOutput Scroll { get; }
        public InputUpdater Updater { get; }
        void Transform(Func<Matrix> getUITransform);
    }
}
