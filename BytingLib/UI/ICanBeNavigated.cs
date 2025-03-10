namespace BytingLib.UI
{
    public interface ICanBeNavigated
    {
        bool CanBeNavigated { get; }
        void ActivateFromNavigation();
    }
}
