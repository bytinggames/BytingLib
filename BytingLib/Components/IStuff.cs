namespace BytingLib
{
    public interface IStuff
    {
        void Add(object component, Action<object>? onRemove = null);
        void Remove(object component);
        IEnumerable<T> Get<T>();

        void ForEach<T>(Action<T> doAction);
    }
}