namespace BytingLib
{
    public class InputCanvas : Input, IInputCanvas
    {
        public Vector2Output MousePosition { get; } = new(new Vector2Mouse());
        public BoolOutput LeftClick { get; } = MouseButton.Left;
        public IntOutput Scroll { get; } = new(new IntMouseWheel());
        public InputUpdater Updater => updater;

        public void Transform(Func<Matrix> getUITransform)
        {
            MousePosition.Dispose();
            Vector2Input inputSource = (Vector2Input)MousePosition.GetPointerValue()!;
            // TODO: inputSource gets disposed... is that a problem? it gets re-added
            MousePosition.SetPointerValue(new Vector2Transform(inputSource, getUITransform));
            //MousePosition.Initialize(updater);
        }

        public InputCanvas(InputUpdater updater) : base(updater)
        {
        }
    }
}
