namespace BytingLib
{
    public abstract class Input : IDisposable
    {
        public abstract IEnumerable<Input> GetChildren();

        public IEnumerable<Input> GetAllChildren()
        {
            return GetChildren()
                .SelectMany(child => child.GetAllChildren().Prepend(child));
        }
        public IEnumerable<Input> GetAllChildrenAndSelf()
        {
            yield return this;
            foreach (var child in GetAllChildren())
            {
                yield return child;
            }
        }

        public IEnumerable<Input> GetAllChildren(Predicate<Input> stopAt)
        {
            foreach (var item in GetChildren())
            {
                yield return item;
                if (!stopAt(item))
                {
                    foreach (var item2 in item.GetAllChildren(stopAt))
                    {
                        yield return item2;
                    }
                }
            }
        }
        public IEnumerable<Input> GetAllChildrenAndSelf(Predicate<Input> stopAt)
        {
            yield return this;
            if (stopAt(this))
            {
                yield break;
            }
            foreach (var child in GetAllChildren(stopAt))
            {
                yield return child;
            }
        }

        public IEnumerable<Input> GetAllChildrenOnlyReturnStop(Predicate<Input> stopAt)
        {
            foreach (var item in GetChildren())
            {
                if (!stopAt(item))
                {
                    foreach (var item2 in item.GetAllChildrenOnlyReturnStop(stopAt))
                    {
                        yield return item2;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
        public IEnumerable<Input> GetAllChildrenAndSelfOnlyReturnStop(Predicate<Input> stopAt)
        {
            if (stopAt(this))
            {
                yield return this;
                yield break;
            }
            foreach (var child in GetAllChildrenOnlyReturnStop(stopAt))
            {
                yield return child;
            }
        }

        public void Update(InputUpdater updater, FullInput input)
        {
            if (updater.HasAlreadyUpdated(this))
            {
                return;
            }
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

            if (IsRegistered(updater))
            {
                return;
            }

            RegisterSelf(updater);

            if (updateToInitialize)
            {
                UpdateSelf(updater, default);
            }
        }

        public abstract bool IsRegistered(InputUpdater updater);

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
