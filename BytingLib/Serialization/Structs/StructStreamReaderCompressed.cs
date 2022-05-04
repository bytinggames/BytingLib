using System.Runtime.InteropServices;

namespace BytingLib
{
    public class StructStreamReaderCompressed<T> : StructStreamReader<T> where T : struct
    {
        byte[] lastData;
        int lastFrame;

        public StructStreamReaderCompressed(Stream stream, int? startPosition = null) : base(stream, startPosition)
        {
            lastData = new byte[Marshal.SizeOf<T>()];
        }

        protected override int? ReadNextFrameActual()
        {
            int? frameDiff = ReadNextFrameDiff();
            if (frameDiff == null)
                return null;

            int nextFrame = lastFrame + frameDiff.Value;
            lastFrame = nextFrame;
            return nextFrame;
        }

        private int? ReadNextFrameDiff()
        {
            try
            {
                byte frameDiffByte = ReadByte();
                if (frameDiffByte != byte.MaxValue)
                    return frameDiffByte;

                if (!stream.ReadFullBuffer(intBuffer))
                    return null;
                return BitConverter.ToInt32(intBuffer);
            }
            catch (StructPlaybackEndOfStreamException)
            {
                return null;
            }
        }

        protected override object? ReadStructActual()
        {
            byte[] diffBuffer = new byte[Marshal.SizeOf(typeof(T))];

            try
            {
                ReadDiffBuffer(diffBuffer);

                ByteExtension.AddBytes(lastData, diffBuffer, lastData);

                return StructSerializer.Read(lastData, typeof(T));
            }
            catch (StructPlaybackEndOfStreamException)
            {
                return null;
            }
        }

        private void ReadDiffBuffer(byte[] diffBuffer)
        {
            int bufferIndex = 0;
            while (true)
            {
                int typeRead = ReadByte();

                Jau.StructDataType message = (Jau.StructDataType)typeRead;

                switch (message)
                {
                    case Jau.StructDataType.Zeros:
                        bufferIndex += ReadByte();
                        break;
                    case Jau.StructDataType.Difference:
                        byte diff = ReadByte();
                        diffBuffer[bufferIndex] = diff;
                        bufferIndex++;
                        break;
                    case Jau.StructDataType.End:
                        return; // end
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private byte ReadByte()
        {
            int read = stream.ReadByte();
            if (read == -1) // unexpected end of stream
                throw new StructPlaybackEndOfStreamException();
            return (byte)read;
        }

        private class StructPlaybackEndOfStreamException : Exception { }
    }
}
