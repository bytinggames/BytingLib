namespace BytingLib
{
    public abstract class InputUpdate : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected InputUpdater updater { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public abstract IEnumerable<InputUpdate> GetChildren();

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

            if (this is IInputOutput output)
            {
                updater.RemoveOutput(output);
            }
        }

        public abstract void Update(FullInput input);

        public object Clone(Creator creator)
        {
            string serialized = creator.Serialize(this);
            object clone = creator.CreateObject(new ScriptReaderLiteral(serialized), GetType());
            return clone;
        }
    }
}
