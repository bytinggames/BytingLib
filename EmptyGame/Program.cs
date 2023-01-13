
internal class Program
{
    private static void Main()
    {
        using (var game = new EmptyGame.Game1())
        {
            game.Run();
        }
    }
}