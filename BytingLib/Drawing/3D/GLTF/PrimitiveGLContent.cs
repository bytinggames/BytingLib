using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace BytingLib
{
    public class PrimitiveGLContent : PrimitiveGLBase
    {
        public byte[] VertexData { get; }
        public VertexDeclaration VertexDeclaration { get; }
        public int VertexCount { get; }
        public PrimitiveGLIndexInfo? IndexInfo { get; }

        public PrimitiveGLContent(ModelGL model, JsonNode n)
            : base(model, n)
        {
            var attributesObj = n["attributes"]!.AsObject();

            string key = string.Concat(attributesObj.Select(f => f.Key + f.Value));

            VertexData = model.GetVertexData(key, attributesObj, out var _vertexDeclaration, out int _vertexCount);
            VertexDeclaration = _vertexDeclaration;
            VertexCount = _vertexCount;


            JsonNode? t;
            if ((t = n["indices"]) != null)
            {
                int indicesAccessorIndex = t.GetValue<int>();

                IndexInfo = new PrimitiveGLIndexInfo(
                    model.GetIndexData(indicesAccessorIndex, out var _indexElementSize, out int _indexCount),
                    _indexElementSize,
                    _indexCount);
            }
        }

        public T[] GetElementData<T>(VertexElementUsage usage) where T : struct
        {
            T[] elements = new T[VertexCount];
            GetElementData(elements, usage);
            return elements;
        }

        /// <summary>array must be at minimum size of VertexCount</summary>
        public void GetElementData<T>(T[] elements, VertexElementUsage usage) where T : struct
        {
            var vertexElements = VertexDeclaration.GetVertexElements();
            var vertexElement = vertexElements.First(f => f.VertexElementUsage == usage);

            // Copy from the temporary buffer to the destination array
            var dataHandle = GCHandle.Alloc(VertexData, GCHandleType.Pinned);
            try
            {
                var tmpPtr = dataHandle.AddrOfPinnedObject();
                tmpPtr = (IntPtr)(tmpPtr.ToInt64() + vertexElement.Offset);
                for (var i = 0; i < VertexCount; i++)
                {
                    elements[i] = Marshal.PtrToStructure<T>(tmpPtr);
                    tmpPtr = (IntPtr)(tmpPtr.ToInt64() + VertexDeclaration.VertexStride);
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public IEnumerable<Triangle3> GetTriangles()
        {
            if (IndexInfo == null)
                yield break;

            Vector3[] positions = GetElementData<Vector3>(VertexElementUsage.Position);
            object indicesObj = IndexInfo.GetIndices();
            if (indicesObj is short[] ind16)
            {
                for (int i = 0; i < ind16.Length; i += 3)
                    yield return new Triangle3(positions[ind16[i]], positions[ind16[i + 2]], positions[ind16[i + 1]]);
            }
            else if (indicesObj is int[] ind32)
            {
                for (int i = 0; i < ind32.Length; i += 3)
                    yield return new Triangle3(positions[ind32[i]], positions[ind32[i + 2]], positions[ind32[i + 1]]);
            }
        }
    }
}
