namespace BytingLib
{
    /// <summary>Don't forget to dispose</summary>
    public class InputCanvasTransformed : Input, IInputCanvas
    {
        private readonly IInputCanvas input;
        public InputUpdater Updater => input.Updater;

        public Vector2Output MousePosition { get; }
        public BoolOutput LeftClick => input.LeftClick;
        public IntOutput Scroll => input.Scroll;

        public InputCanvasTransformed(IInputCanvas input, Func<Matrix> getUITransform)
            : base(input.Updater, false)
        {
            this.input = input;

            MousePosition = new Vector2Output(new Vector2Transform(input.MousePosition, getUITransform));

            InitializeOutputs();
        }

        public void Transform(Func<Matrix> getUITransform)
        {
            throw new NotImplementedException();
        }
    }
}
