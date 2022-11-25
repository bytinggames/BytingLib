
namespace BytingLib
{
    public class MetaInput : IUpdate
    {
        private readonly Func<MetaInputState> getState;

        private MetaInputState previousState;
        private MetaInputState currentState;

        public delegate void StateChanged(MetaInputState current, MetaInputState previous);
        public event StateChanged? OnStateChanged;

        public MetaInput(Func<MetaInputState> getState)
        {
            this.getState = getState;
        }

        public bool IsActivatedThisUpdate => currentState.IsActivatedThisUpdate;

        public void Update()
        {
            UpdateUsingState(getState());
        }

        private void UpdateUsingState(MetaInputState newState)
        {
            previousState = currentState;
            currentState = newState;

            if (OnStateChanged != null
                && currentState != previousState)
                OnStateChanged?.Invoke(currentState, previousState);
        }
    }
}
