using System.Collections.ObjectModel;

namespace BytingLib
{
    public class StuffDisposable : Stuff, IDisposable, IStuffDisposable
    {
        private readonly List<object> allThings = new List<object>();

        public event Action? OnDispose;

        public ReadOnlyCollection<object> AllThings => allThings.AsReadOnly();

        public bool CheckIfAnyInterfaceIsInherited { get; set; } = true;

        public StuffDisposable(params Type[] types)
            : base(false, types)
        { }

        public override void Add(object thing, Action<object>? onRemove = null)
        {
            bool any = thing is IDisposable;

            any = AddActual(thing, onRemove) || any;

            if (!any && CheckIfAnyInterfaceIsInherited)
                throw new ArgumentException(thing.GetType() + " didn't inherit any provided interface.");

            allThings.Add(thing);
        }

        public override void Remove(object thing)
        {
            if (!allThings.Remove(thing))
                return; //throw new ArgumentException("thing wasn't contained in stuff.");

            if (thing is IDisposable disposable)
                disposable.Dispose();

            RemoveActual(thing);
        }

        public virtual void Dispose()
        {
            while (allThings.Count > 0)
            {
                Remove(allThings[allThings.Count - 1]);
            }

            if (listsOfThings.Count > 0
                && listsOfThings.Any(f => f.Value.Count > 0))
                throw new BytingException("listsOfThings aren't empty after disposal.");

            OnDispose?.Invoke();
        }

        public void Insert(int index, object thing, Action<object>? onRemove = null)
        {
            bool any = thing is IDisposable;

            any = InsertActual(index, thing) || any;

            if (!any && CheckIfAnyInterfaceIsInherited)
                throw new ArgumentException(thing.GetType() + " didn't inherit any provided interface.");

            if (onRemove != null)
            {
                if (!onRemoveActions.TryAdd(thing, onRemove))
                    throw new ArgumentException("The thing has been added before to the onRemoveActions dictionary. This is not supported.");
            }

            allThings.Insert(index, thing);
        }

        private bool InsertActual(int index, object thing)
        {
            bool any = false;
            foreach (var match in GetMatchingLists(thing))
            {
                // check on the allThings list, where the last thing is, that also inherits the interface of this match
                int i = index - 1;
                for (; i >= 0; i--)
                {
                    if (allThings[i].GetType().IsAssignableTo(match.Type))
                    {
                        break;
                    }
                }
                if (i >= 0)
                {
                    i = match.List.IndexOf(allThings[i]);
                }
                i++;

                match.List.Insert(i, thing);
                any = true;
            }

            return any;
        }
    }
}