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
        public static object RemoveAt(this IStuffDisposable stuff, int index)
        {
            return stuff.AllThings.RemoveAtGet(index);
        }
    }
}