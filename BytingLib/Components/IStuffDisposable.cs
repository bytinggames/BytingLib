using System.Collections.ObjectModel;

namespace BytingLib
{
    public interface IStuffDisposable : IStuff, IDisposable
    {
        ReadOnlyCollection<object> AllThings { get; }
        void Insert(int index, object thing, Action<object>? onRemove = null);
    }

    public static class IStuffDisposableExtension
    {
        // TODO: this method performance could be improved by directly removing from index inside the implementation of the interface
        public static void RemoveAt(this IStuffDisposable stuff, int index)
        {
            object obj = stuff.AllThings[index];
            stuff.Remove(obj);
        }
    }
}