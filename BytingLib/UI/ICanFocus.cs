namespace BytingLib.UI
{
    public interface ICanFocus
    {
        bool CanFocus { get; }
        void ClickFromFocus();
    }
}
