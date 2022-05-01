namespace BytingLib
{
    public abstract class StructStreamWriter<T> : IStructStreamWriter<T> where T : struct
    {
        protected readonly Stream stream;

        T lastState;
        int frame = -1;
        protected int Frame => frame;
        private bool isDisposed;

        public StructStreamWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void AddState(T state)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(StructStreamWriter<T>));

            frame++;
            if (!state.Equals(lastState))
            {
                OnStateChanged(state, lastState);
                lastState = state;
            }
        }

        private void OnStateChanged(T currentState, T previousState)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(StructStreamWriter<T>));

            WriteFrame();
            WriteStructChange(currentState, previousState);
        }

        public void Dispose()
        {
            isDisposed = true;
            WriteFrame();
        }

        protected virtual void WriteFrame()
        {
            stream.Write(BitConverter.GetBytes(frame));
        }

        protected virtual void WriteStructChange(T currentState, T previousState)
        {
            StructSerializer.Write(stream, currentState);
        }
    }
}
