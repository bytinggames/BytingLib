namespace BytingLib
{
    public interface IStuffDisposable : IStuff, IDisposable
    {
        IEnumerable<object> AllThings { get; }
    }
}