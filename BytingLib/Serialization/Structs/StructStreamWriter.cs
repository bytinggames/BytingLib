namespace BytingLib.Serialization
{
    public abstract class StructStreamWriter<T> : IStructStreamWriter<T> where T : struct
    {
        protected readonly Stream stream;
        protected T lastState;
        protected int frame = -1;
        private readonly bool alwaysFlush;
        private bool isDisposed;

        public StructStreamWriter(Stream stream, bool alwaysFlush)
        {
            this.stream = stream;
            this.alwaysFlush = alwaysFlush;
        }

        public void AddState(T state)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(StructStreamWriter<T>));
            }

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
            {
                throw new ObjectDisposedException(nameof(StructStreamWriter<T>));
            }

            WriteFrame();
            WriteStructChange(currentState, previousState);
            if (alwaysFlush)
            {
                stream.Flush();
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                WriteFrame();
            }
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
