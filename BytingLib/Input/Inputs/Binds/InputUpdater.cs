namespace BytingLib
{
    public class InputUpdater : IUpdate
    {
        public long CurrentStamp { get; private set; }
        protected readonly Func<FullInput> getFullInput;
        private readonly List<Input> outputs = new();

        public InputUpdater(Func<FullInput> getFullInput)
        {
            this.getFullInput = getFullInput;
        }

        public void Update()
        {
            CurrentStamp++;

            var input = getFullInput();
            for (int i = 0; i < outputs.Count; i++)
            {
                outputs[i].Update(this, input);
            }
        }

        public bool AddOutput(Input output)
        {
            if (!outputs.Contains(output))
            {
                outputs.Add(output);

                output.Register(this);

                return true;
            }
            return false;
        }

        public void RemoveOutput(Input output)
        {
            outputs.Remove(output);
        }
    }
}
