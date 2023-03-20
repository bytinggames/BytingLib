using System.Runtime.InteropServices;

namespace BytingLib
{
    public class PrimitiveGLIndexInfo
    {
        public byte[] IndexData { get; }
        public IndexElementSize IndexElementSize { get; }
        public int IndexCount { get; }

        public PrimitiveGLIndexInfo(byte[] indexData, IndexElementSize indexElementSize, int indexCount)
        {
            IndexData = indexData;
            IndexElementSize = indexElementSize;
            IndexCount = indexCount;
        }

        /// <summary>Return type is either short[] or int[] depending of bit size of the index buffer elements.</summary>
        public object GetIndices()
        {
            switch (IndexElementSize)
            {
                case IndexElementSize.SixteenBits:
                    short[] indicesShort = new short[IndexCount];
                    GetIndices(indicesShort);
                    return indicesShort;
                case IndexElementSize.ThirtyTwoBits:
                    int[] indicesInt = new int[IndexCount];
                    GetIndices(indicesInt);
                    return indicesInt;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>indices array must be at minimum size of VertexCount. The index buffer must contain 16bit indices.</summary>
        public void GetIndices(short[] indices)
        {
            // Copy from the temporary buffer to the destination array
            var dataHandle = GCHandle.Alloc(IndexData, GCHandleType.Pinned);
            try
            {
                var tmpPtr = dataHandle.AddrOfPinnedObject();

                if (IndexElementSize == IndexElementSize.ThirtyTwoBits)
                    throw new BytingException("t array must be of type int when reading from a 32 bit index buffer");

                int stride = 2;
                for (var i = 0; i < IndexCount; i++)
                {
                    indices[i] = Marshal.PtrToStructure<short>(tmpPtr);
                    tmpPtr = (IntPtr)(tmpPtr.ToInt64() + stride);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        /// <summary>indices array must be at minimum size of VertexCount.</summary>
        public void GetIndices(int[] indices)
        {
            // Copy from the temporary buffer to the destination array
            var dataHandle = GCHandle.Alloc(IndexData, GCHandleType.Pinned);
            try
            {
                var tmpPtr = dataHandle.AddrOfPinnedObject();

                if (IndexElementSize == IndexElementSize.ThirtyTwoBits)
                    throw new BytingException("t array must be of type int when reading from a 32 bit index buffer");

                int stride = IndexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2;

                switch (IndexElementSize)
                {
                    case IndexElementSize.SixteenBits:
                        for (var i = 0; i < IndexCount; i++)
                        {
                            indices[i] = Marshal.PtrToStructure<short>(tmpPtr);
                            tmpPtr = (IntPtr)(tmpPtr.ToInt64() + stride);
                        }
                        break;
                    case IndexElementSize.ThirtyTwoBits:
                        for (var i = 0; i < IndexCount; i++)
                        {
                            indices[i] = Marshal.PtrToStructure<int>(tmpPtr);
                            tmpPtr = (IntPtr)(tmpPtr.ToInt64() + stride);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }


            }
            finally
            {
                dataHandle.Free();
            }
        }
    }
}
