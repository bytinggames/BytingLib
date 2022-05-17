
namespace BytingLib
{
    public class StructChannel<T> where T : struct
    {
        private readonly Func<T> getState;
        private List<IStructMatcher<T>> catchers = new List<IStructMatcher<T>>();

        public StructChannel(Func<T> getState)
        {
            this.getState = getState;
        }

        public void AddCatcher(IStructMatcher<T> catcher)
        {
            catchers.Add(catcher);
        }

        public void RemoveCatcher(IStructMatcher<T> catcher)
        {
            catchers.Remove(catcher);
        }

        public T GetState()
        {
            T state = getState();

            // struct to bytes
            byte[] stateBytes = StructSerializer.GetBytes(state);

            if (catchers.Count > 0)
            {
                List<IStructMatcher<T>> catched = catchers.Where(c => c.DoesMatch(stateBytes)).ToList();

                foreach (var c in catched)
                {
                    c.Match(stateBytes);
                }

                if (catched.Count > 0)
                {
                    // convert modified bytes back to struct
                    state = (T)StructSerializer.Read(stateBytes, typeof(T));
                }
            }

            return state;
        }
    }
}
