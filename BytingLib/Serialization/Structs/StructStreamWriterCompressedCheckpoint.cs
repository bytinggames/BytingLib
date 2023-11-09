namespace BytingLib.Serialization
{
    public class StructStreamWriterCompressedCheckpoint<T> : StructStreamWriterCompressed<T> where T : struct
    {
        byte[]? previousDataCheckpoint;
        int lastFrameCheckpoint;
        int frameCheckpoint;
        T lastStateCheckpoint;
        long streamPositionCheckpoint;

        public StructStreamWriterCompressedCheckpoint(Stream stream, bool alwaysFlush) : base(stream, alwaysFlush)
        {
        }

        public void StoreCheckpoint()
        {
            previousDataCheckpoint = previousData;
            lastFrameCheckpoint = lastFrame;
            frameCheckpoint = frame;
            lastStateCheckpoint = lastState;
            streamPositionCheckpoint = stream.Position;
        }

        public void LoadCheckpoint()
        {
            if (previousDataCheckpoint == null)
            {
                throw new Exception("previousDataCheckpoint shouldn't be null. Make sure StoreCheckpoint() is called before LoadCheckpoint()");
            }

            previousData = previousDataCheckpoint;
            lastFrame = lastFrameCheckpoint;
            frame = frameCheckpoint;
            lastState = lastStateCheckpoint;

            // set stream position to checkpoint stream position
            // clear everything after that stream position
            stream.SetLength(streamPositionCheckpoint);
        }
    }
}
