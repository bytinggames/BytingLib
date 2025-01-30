namespace BytingLib
{
    public abstract class InputUpdate : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected InputUpdater updater { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public abstract IEnumerable<InputUpdate> GetChildren();


        public IEnumerable<InputUpdate> GetAllRecursively()
        {
            foreach (var child in GetChildren())
            {
                foreach (var c in child.GetAllRecursively())
                {
                    yield return c;
                }
            }
            yield return this;
        }

        public void Initialize(InputUpdater updater)
        {
            this.updater = updater;

            foreach (var child in GetChildren())
            {
                child.Initialize(updater);
            }
        }

        public void Dispose()
        {
            foreach (var child in GetChildren())
            {
                child.Dispose();
            }
        }

        public abstract void Update(FullInput input);
    }
}
