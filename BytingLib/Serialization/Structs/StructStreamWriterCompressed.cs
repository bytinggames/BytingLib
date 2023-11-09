using System.Runtime.InteropServices;

namespace BytingLib.Serialization
{
    public class StructStreamWriterCompressed<T> : StructStreamWriter<T> where T : struct
    {
        protected byte[] previousData;
        protected int lastFrame;

        public StructStreamWriterCompressed(Stream stream, bool alwaysFlush)
            : base(stream, alwaysFlush)
        {
            previousData = new byte[Marshal.SizeOf<T>()];
        }

        protected override void WriteFrame()
        {
            int diff = frame - lastFrame;

            if (diff >= byte.MinValue && diff < byte.MaxValue)
            {
                // this is the case that happens most of the time, so we will only use 1 byte for that
                stream.WriteByte((byte)diff);
            }
            else
            {
                stream.WriteByte(byte.MaxValue); // marks that an int is coming next that contains the real diff
                stream.Write(BitConverter.GetBytes(diff));
            }

            lastFrame = frame;
        }

        protected override void WriteStructChange(T currentState, T previousState)
        {
            byte[] currentData = StructSerializer.GetBytes(currentState);
            byte[] difference = new byte[currentData.Length];
            ByteExtension.SubtractBytes(currentData, previousData, difference);
            previousData = currentData;

            byte zeros = 0;
            for (int i = 0; i < difference.Length; i++)
            {
                if (difference[i] == 0)
                {
                    zeros++;
                    if (zeros == byte.MaxValue)
                    {
                        WriteZeros(ref zeros);
                    }
                }
                else
                {
                    if (zeros > 0)
                    {
                        WriteZeros(ref zeros);
                    }

                    // write change
                    WriteDifference(difference[i]);
                }
            }
            WriteEnd();
        }

        private void WriteZeros(ref byte zeros)
        {
            stream.WriteByte((byte)StructStreamDataType.Zeros);
            stream.WriteByte(zeros);
            zeros = 0;
        }

        private void WriteDifference(byte change)
        {
            stream.WriteByte((byte)StructStreamDataType.Difference);
            stream.WriteByte(change);
        }

        private void WriteEnd()
        {
            stream.WriteByte((byte)StructStreamDataType.End);
        }

        //enum DataType : byte
        //{
        //    WholeStruct = 0,
        //    Difference = 1,
        //}

    }
}
