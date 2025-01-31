namespace BytingLib
{
    public class InputCanvas : Input, IInputCanvas
    {
        public Vector2Output MousePosition { get; } = new(new Vector2Mouse());
        public BoolOutput LeftClick { get; } = MouseButton.Left;
        public IntOutput Scroll { get; } = new(new IntMouseWheel());
        public InputUpdater Updater => updater;

        public void SetMousePosition(Vector2 position)
        {
            Microsoft.Xna.Framework.Input.Mouse.SetPosition(
                (int)MathF.Round(position.X),
                (int)MathF.Round(position.Y)
            );
        }

        public InputCanvas(InputUpdater updater) : base(updater)
        {
        }
    }
}
