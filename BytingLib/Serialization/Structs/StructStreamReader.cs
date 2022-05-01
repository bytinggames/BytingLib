using System.Collections;

namespace BytingLib
{
    public abstract class StructStreamReader<T> : IEnumerator<T> where T : struct
    {
        protected readonly Stream stream;
        private readonly int? startPosition;
        private int currentFrame;
        private int frameWithNextStateChange;
        protected byte[] intBuffer = new byte[4];
        object IEnumerator.Current => Current;

        public T Current { get; private set; }
        public bool IsFinished { get; private set; }

        public StructStreamReader(Stream stream, int? startPosition = null)
        {
            this.stream = stream;
            this.startPosition = startPosition;
            Initialize();
        }

        private void Initialize()
        {
            if (startPosition.HasValue)
                stream.Position = startPosition.Value;
            currentFrame = -1;
            frameWithNextStateChange = -1;
            Current = default;
            IsFinished = false;

            ReadNextFrame();
        }

        private void ReadNextFrame()
        {
            int? newFrame = ReadNextFrameActual();

            if (newFrame != null)
            {
                if (newFrame == frameWithNextStateChange)
                    OnFinish();
                else
                    frameWithNextStateChange = newFrame.Value;
            }
            else
            {
                OnFinish();
            }
        }

        protected virtual int? ReadNextFrameActual()
        {
            if (!stream.ReadFullBuffer(intBuffer))
                return null;
            return BitConverter.ToInt32(intBuffer);
        }

        private void OnFinish()
        {
            IsFinished = true;
        }

        // returns false if the struct can't be read from the current frame -> the playback has ended
        private bool ReadStruct()
        {
            object? readObject = ReadStructActual();

            if (readObject == null)
            {
                OnFinish();
                return false;
            }

            Current = (T)readObject;

            ReadNextFrame();

            return true;
        }

        protected virtual object? ReadStructActual()
        {
            return StructSerializer.Read(stream, typeof(T));
        }

        public bool MoveNext()
        {
            currentFrame++;

            if (IsFinished)
                return false;

            if (currentFrame == frameWithNextStateChange)
                ReadStruct();

            return true;
        }

        public void Reset()
        {
            if (startPosition == null)
                throw new StructPlaybackException("Reset() is only supported, if startPosition is not null. Also make sure, that the position of the stream can be changed.");

            Initialize();
        }

        public void Dispose()
        {
        }
    }
}
