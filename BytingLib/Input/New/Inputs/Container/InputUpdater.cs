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

        public void AddOutput(InputUpdate output)
        {
            outputs.Add(output);
        }

        internal void RemoveOutput(InputUpdate output)
        {
            outputs.Remove(output);
        }
    }
}
