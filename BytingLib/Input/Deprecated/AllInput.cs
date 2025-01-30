namespace BytingLib
{
    public record AllInput(KeyInput Keys, MouseInput Mouse, GamePadInput GamePad, Func<Vector2> GetCustomMouseMovement);
}