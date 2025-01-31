namespace BytingLib
{
    /// <summary>Don't forget to dispose</summary>
    public class InputCanvasTransformed : Input, IInputCanvas
    {
        private readonly IInputCanvas input;
        private readonly Func<Matrix> getUITransform;
        public InputUpdater Updater => input.Updater;

        public Vector2Output MousePosition { get; }
        public BoolOutput Click => input.Click;
        public IntOutput Scroll => input.Scroll;

        public InputCanvasTransformed(IInputCanvas input, Func<Matrix> getUITransform)
            : base(input.Updater, false)
        {
            this.input = input;
            this.getUITransform = getUITransform;
            MousePosition = new Vector2Output(new Vector2Transform(input.MousePosition, getUITransform));

            InitializeOutputs();
        }

        public void SetMousePosition(Vector2 position)
        {
            position = Vector2.Transform(position, Matrix.Invert(getUITransform()));
            input.SetMousePosition(position);
        }
    }
}
