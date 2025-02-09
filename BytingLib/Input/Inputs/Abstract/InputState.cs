namespace BytingLib
{
    public abstract class InputState<T>(InputUpdater updater)
    {
        public InputUpdater Updater { get; } = updater;

        public abstract void Update(T outputState);
    }
}
