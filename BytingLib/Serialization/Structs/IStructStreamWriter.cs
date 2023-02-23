namespace BytingLib.Serialization
{
    public interface IStructStreamWriter<T> : IDisposable where T : struct
    {
        void AddState(T state);
    }
}
