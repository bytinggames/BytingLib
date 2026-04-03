namespace BytingLib.Serialization
{
    public class StructStreamReaderCompressedCheckpoint<T> : StructStreamReaderCompressed<T> where T : struct
    {
        byte[]? lastDataCheckpoint;
        int lastFrameCheckpoint;
        int currentFrameCheckpoint;
        int frameWithNextStateChangeCheckpoint;
        T currentCheckpoint;
        bool isFinishedCheckpoint;
        long streamPositionCheckpoint;

        public StructStreamReaderCompressedCheckpoint(Stream stream, int? startPosition = null, int[,]? insertZeroesForMigration = null) : base(stream, startPosition, insertZeroesForMigration)
        {
        }

        public void StoreCheckpoint()
        {
            lastDataCheckpoint = lastData.ToArray();
            lastFrameCheckpoint = lastFrame;
            currentFrameCheckpoint = CurrentFrame;
            frameWithNextStateChangeCheckpoint = frameWithNextStateChange;
            currentCheckpoint = Current;
            isFinishedCheckpoint = IsFinished;
            streamPositionCheckpoint = stream.Position;
        }

        public void LoadCheckpoint()
        {
            if (lastDataCheckpoint == null)
            {
                throw new Exception("call StoreCheckpoint() first");
            }
            lastData = lastDataCheckpoint.ToArray();
            lastFrame = lastFrameCheckpoint;
            CurrentFrame = currentFrameCheckpoint;
            frameWithNextStateChange = frameWithNextStateChangeCheckpoint;
            Current = currentCheckpoint;
            IsFinished = isFinishedCheckpoint;
            stream.Position = streamPositionCheckpoint;
        }
    }
}
