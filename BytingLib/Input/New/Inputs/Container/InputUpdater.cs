namespace BytingLib
{
    public class InputUpdater : IUpdate
    {
        public long CurrentStamp { get; private set; }
        private readonly Func<FullInput> getFullInput;
        public Action<Exception> OnException { get; }
        private readonly List<InputUpdate> outputs = new();

        public InputUpdater(Func<FullInput> getFullInput, Action<Exception> onException)
        {
            this.getFullInput = getFullInput;
            this.OnException = onException;
        }

        public void Update()
        {
            CurrentStamp++;

            var input = getFullInput();
            for (int i = 0; i < outputs.Count; i++)
            {
                foreach (var child in outputs[i].GetAllRecursively())
                {
                    child.Update(input);
                }
            }
        }

        public bool AddOutput(InputUpdate output)
        {
            if (!outputs.Contains(output))
            {
                outputs.Add(output);
                return true;
            }
            return false;
        }

        internal void RemoveOutput(InputUpdate output)
        {
            outputs.Remove(output);
        }
    }
}
