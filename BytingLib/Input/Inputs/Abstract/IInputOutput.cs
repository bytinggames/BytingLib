namespace BytingLib
{
    public interface IInputOutput : IPointerValue
    {
        IEnumerable<InputUpdate> GetAllRecursivelyUntilOutput();
        void Initialize(InputUpdater updater);
        void Disable();
    }
}
