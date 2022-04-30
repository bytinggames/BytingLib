namespace BytingLib
{
    public class StuffDisposable : Stuff, IDisposable, IStuffDisposable
    {
        private readonly List<object> allThings = new List<object>();

        public event Action? OnDispose;

        public StuffDisposable(params Type[] types)
            : base(false, types)
        { }

        public override void Add(object thing, Action<object>? onRemove = null)
        {
            allThings.Add(thing);

            bool any = thing is IDisposable;

            any = AddActual(thing, onRemove) || any;

            if (!any)
                throw new ArgumentException("The thing didn't inherit any provided interface.");
        }

        public override void Remove(object thing)
        {
            bool any = false;
            if (thing is IDisposable disposable)
            {
                disposable.Dispose();
                any = true;
            }

            any = RemoveActual(thing) || any;

            if (!any)
                throw new ArgumentException("The thing didn't inherit any provided interface.");
        }

        public virtual void Dispose()
        {
            while (allThings.Count > 0)
            {
                int index = allThings.Count - 1;
                Remove(allThings[index]);
                allThings.RemoveAt(index);
            }

            if (listsOfThings.Count > 0
                && listsOfThings.Any(f => f.Value.Count > 0))
                throw new BytingException("listsOfThings aren't empty after disposal.");

            OnDispose?.Invoke();
        }
    }
}