namespace BytingLib
{
    public interface IStructMatcher<T>
    {
        void Match(byte[] stateBytes);
        bool DoesMatch(byte[] stateBytes);
    }
}