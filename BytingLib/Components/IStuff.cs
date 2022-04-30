namespace BytingLib
{
    public interface IStuff
    {
        void Add(object component, Action<object>? onRemove = null);
        void AddRange(params object[] components);
        void Remove(object component);
        void RemoveRange(params object[] component);
        IReadOnlyCollection<T> Get<T>();
        void ForEach<T>(Action<T> doAction);
    }
}