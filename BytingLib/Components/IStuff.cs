namespace BytingLib
{
    public interface IStuff
    {
        void Add(object thing, Action<object>? onRemove = null);
        void Remove(object thing);
        IReadOnlyCollection<T> Get<T>();
        void ForEach<T>(Action<T> doAction);
    }

    public static class IStuffExtension
    {
        public static void AddRange(this IStuff stuff, params object[] things)
        {
            foreach (var thing in things)
                stuff.Add(thing);
        }

        public static void RemoveRange(this IStuff stuff, params object[] things)
        {
            foreach (var thing in things)
                stuff.Remove(thing);
        }
    }
}