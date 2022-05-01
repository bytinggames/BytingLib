using System.Collections;

namespace BytingLib
{
    public class Stuff : IStuff
    {
        protected readonly Dictionary<Type, IList> listsOfThings = new();
        protected Dictionary<object, Action<object>> onRemoveActions = new();
        private readonly List<Iteration> iterations = new List<Iteration>();

        public Stuff(params Type[] types)
            : this(true, types)
        {
        }

        protected Stuff(bool mustHaveOneEntry, params Type[] types)
        {
            if (mustHaveOneEntry &&
                (types == null || types.Length == 0))
                throw new ArgumentException("Types must have at least one entry.");

            if (types != null)
            {
                foreach (Type t in types)
                {
                    if (!t.IsInterface)
                        throw new ArgumentException("Only interfaces are supported.");
                    Type genericListType = typeof(List<>).MakeGenericType(t);
                    listsOfThings.Add(t, (IList)Activator.CreateInstance(genericListType)!);
                }
            }
        }

        public virtual void Add(object thing, Action<object>? onRemove = null)
        {
            if (thing is null) throw new ArgumentNullException(nameof(thing));

            bool any = AddActual(thing, onRemove);
            if (!any)
                throw new ArgumentException("The thing didn't inherit any provided interface.");
        }

        protected virtual bool AddActual(object thing, Action<object>? onRemove)
        {
            bool any = false;
            foreach (var match in GetMatchingLists(thing))
            {
                match.List.Add(thing);
                any = true;
            }

            if (onRemove != null)
            {
                if (!onRemoveActions.TryAdd(thing, onRemove))
                    throw new ArgumentException("The thing has been added before to the onRemoveActions dictionary. This is not supported.");
            }

            return any;
        }

        public virtual void AddRange(params object[] things)
        {
            foreach (var thing in things)
                Add(thing);
        }

        public virtual void Remove(object thing)
        {
            bool any = RemoveActual(thing);
            if (!any)
                throw new ArgumentException("The thing didn't inherit any provided interface.");
        }

        public virtual void RemoveRange(params object[] things)
        {
            foreach (var thing in things)
                Remove(thing);
        }

        protected bool RemoveActual(object thing)
        {
            bool any = false;
            foreach (var match in GetMatchingLists(thing))
            {
                var index = match.List.IndexOf(thing);
                if (index != -1)
                {
                    match.List.RemoveAt(index);
                    foreach (var iteration in iterations.Where(f => f.InterfaceType == match.Type))
                    {
                        if (iteration.Index >= index)
                            iteration.Index--;
                    }
                }
                any = true;
            }

            if (onRemoveActions.TryGetValue(thing, out Action<object>? action))
            {
                onRemoveActions.Remove(thing);
                action!.Invoke(thing);
            }

            return any;
        }

        public virtual IEnumerable<(Type Type, IList List)> GetMatchingLists(object thing)
        {
            // for each interface that is inherited, add to the corresponding list
            var interfaces = thing.GetType().GetInterfaces();

            var types = interfaces;
            foreach (var t in types)
            {
                if (listsOfThings.TryGetValue(t, out IList? list))
                {
                    yield return (t, list);
                }
            }
        }

        public virtual IReadOnlyCollection<T> Get<T>()
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("Type is no interface.");

            if (!listsOfThings.TryGetValue(typeof(T), out IList? list))
                throw new ArgumentException("There is no list of generic type " + typeof(T) + " inside this collection.");

            return (list as List<T>)!.AsReadOnly();
        }

        public void ForEach<T>(Action<T> doAction)
        {
            IReadOnlyCollection<T> list = Get<T>();
            
            Iteration iteration = new Iteration(typeof(T));
            iterations.Add(iteration);
            try
            {
                for (; iteration.Index < list.Count; iteration.Index++)
                {
                    doAction(list.ElementAt(iteration.Index));
                }
            }
            finally
            {
                iterations.Remove(iteration);
            }
        }

        class Iteration
        {
            public Type InterfaceType { get; }
            public int Index { get; set; } = 0;

            public Iteration(Type interfaceType)
            {
                InterfaceType = interfaceType;
            }
        }

    }
}