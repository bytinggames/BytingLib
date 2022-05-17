namespace BytingLib
{
    public interface IStructListener<T>
    {
        void OnMatch(byte[] stateBytes);
        bool DoesMatch(byte[] stateBytes);
    }
}