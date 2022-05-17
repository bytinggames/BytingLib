
namespace BytingLib
{
    public class StructChannel<T> where T : struct
    {
        private readonly Func<T> getState;
        private List<IStructListener<T>> listeners = new List<IStructListener<T>>();

        public StructChannel(Func<T> getState)
        {
            this.getState = getState;
        }

        public void AddListener(IStructListener<T> listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IStructListener<T> listener)
        {
            listeners.Remove(listener);
        }

        public T GetState()
        {
            T state = getState();

            // struct to bytes
            byte[] stateBytes = StructSerializer.GetBytes(state);

            if (listeners.Count > 0)
            {
                List<IStructListener<T>> catched = listeners.Where(c => c.DoesMatch(stateBytes)).ToList();

                foreach (var c in catched)
                {
                    c.OnMatch(stateBytes);
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
