namespace BytingLib
{
    public interface IStructStreamWriter<T> : IDisposable where T : struct
    {
        void AddState(T state);
    }
}
