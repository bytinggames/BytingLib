using System.Runtime.InteropServices;

namespace BytingLib.Serialization
{
    public class StructStreamReaderCompressed<T> : StructStreamReader<T> where T : struct
    {
        protected byte[] lastData;
        protected int lastFrame;
        private readonly int[,]? insertZeroesForMigration;

        public StructStreamReaderCompressed(Stream stream, int? startPosition = null, int[,]? insertZeroesForMigration = null) : base(stream, startPosition)
        {
            lastData = new byte[Marshal.SizeOf<T>()];
            
            if (insertZeroesForMigration != null 
                && insertZeroesForMigration.GetLength(0) == 0)
            {
                insertZeroesForMigration = null;
            }
            this.insertZeroesForMigration = insertZeroesForMigration;
        }

        protected override int? ReadNextFrameActual()
        {
            int? frameDiff = ReadNextFrameDiff();
            if (frameDiff == null)
            {
                return null;
            }

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
                {
                    return frameDiffByte;
                }

                if (!stream.ReadFullBuffer(intBuffer))
                {
                    return null;
                }

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
                if (insertZeroesForMigration != null)
                {
                    ReadDiffBuffer(diffBuffer, insertZeroesForMigration);
                }
                else
                {
                    ReadDiffBuffer(diffBuffer);
                }
                ByteExtension.AddBytes(lastData, diffBuffer, lastData);

                return StructSerializer.Read(lastData, typeof(T));
            }
            catch (StructPlaybackEndOfStreamException)
            {
                return null;
            }
        }

        private void ReadDiffBuffer(byte[] diffBuffer, int[,] insertZeroesForMigration)
        {
            int bufferIndex = 0;
            int migrationIndex = 0;
            int migrationSteps = insertZeroesForMigration.GetLength(0);
            do
            {
                if (migrationIndex < migrationSteps)
                {
                    if (bufferIndex >= insertZeroesForMigration[migrationIndex, 0])
                    {
                        bufferIndex += insertZeroesForMigration[migrationIndex++, 1];
                        continue;
                    }
                }
            } while (ReadDiffBufferInner(diffBuffer, ref bufferIndex));
        }

        private void ReadDiffBuffer(byte[] diffBuffer)
        {
            int bufferIndex = 0;
            while (ReadDiffBufferInner(diffBuffer, ref bufferIndex)) { }
        }

        private bool ReadDiffBufferInner(byte[] diffBuffer, ref int bufferIndex)
        {
            var message = (StructStreamDataType)ReadByte();

            switch (message)
            {
                case StructStreamDataType.Zeros:
                    bufferIndex += ReadByte();
                    break;
                case StructStreamDataType.Difference:
                    byte diff = ReadByte();
                    diffBuffer[bufferIndex++] = diff;
                    break;
                case StructStreamDataType.End:
                    return false; // end
                default:
                    throw new NotImplementedException();
            }
            return true;
        }

        private byte ReadByte()
        {
            int read = stream.ReadByte();
            if (read == -1) // unexpected end of stream
            {
                throw new StructPlaybackEndOfStreamException();
            }

            return (byte)read;
        }

        private class StructPlaybackEndOfStreamException : Exception { }
    }
}
