
namespace BytingLib
{
    public partial class InputCanvas : IInputCanvas
    {
        public InputUpdater Updater => updater;

        public void SetMousePosition(Vector2 position)
        {
            Microsoft.Xna.Framework.Input.Mouse.SetPosition(
                (int)MathF.Round(position.X),
                (int)MathF.Round(position.Y)
            );
        }
    }
}
