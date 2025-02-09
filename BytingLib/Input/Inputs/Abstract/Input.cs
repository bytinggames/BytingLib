namespace BytingLib
{
    public abstract class Input : IDisposable
    {
        public abstract IEnumerable<Input> GetChildren();

        public void Update(InputUpdater updater, FullInput input)
        {
            foreach (var child in GetChildren())
            {
                child.Update(updater, input);
            }

            UpdateSelf(updater, input);
        }
        protected abstract void UpdateSelf(InputUpdater updater, FullInput input);

        public void Register(InputUpdater updater, bool updateToInitialize)
        {
            foreach (var child in GetChildren())
            {
                child.Register(updater, updateToInitialize);
            }

            RegisterSelf(updater);
            if (updateToInitialize)
            {
                UpdateSelf(updater, default);
            }
        }
        protected abstract void RegisterSelf(InputUpdater updater);

        public abstract List<InputUpdater> GetUpdaters();

        public void Override<T>(T newInput, Action<T> setNewInput) where T : Input
        {
            List<InputUpdater> updaters = GetUpdaters();

            Unregister(updaters);

            foreach (var updater in updaters)
            {
                newInput.Register(updater, false);
            }
            setNewInput(newInput);
        }

        public void Unregister(List<InputUpdater> updaters)
        {
            foreach (var child in GetChildren())
            {
                child.Unregister(updaters);
            }

            UnregisterSelf(updaters);
        }

        public void Dispose()
        {
            List<InputUpdater> updaters = GetUpdaters();
            foreach (var updater in updaters)
            {
                updater.RemoveOutput(this);
            }
            Unregister(updaters);

            DisposeSelf();
        }

        protected virtual void DisposeSelf() { }
        protected abstract void UnregisterSelf(List<InputUpdater> updaters);
    }

}
