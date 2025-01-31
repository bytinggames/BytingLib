namespace BytingLib
{
    public interface IInputOutput
    {
        IEnumerable<InputUpdate> GetAllRecursivelyUntilOutput();
        void Initialize(InputUpdater updater);
    }
}
