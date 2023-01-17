
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public abstract class MeshBatcher
    {
        public abstract void Begin();

        /// <summary>Returns false, when batch was empty</summary>
        public abstract bool End(RenderSettings settings);
    }

    public class MeshBatcher<V> : MeshBatcher where V : struct, IVertexType
    {
        private const int InitialArraySize = 64;

        // made internal for faster accessing for better performance
        internal V[] vertices;
        internal int[] indices;
        internal int verticesIndex;
        internal int indicesIndex;

        public MeshBatcher()
        {
            vertices = new V[InitialArraySize];
            indices = new int[InitialArraySize];
        }

        //public void Add(IList<V> newVertices)
        //{
        //    EnsureAdditionalArrayCapacity(newVertices.Count, newVertices.Count);

        //    for (int i = 0; i < newVertices.Count; i++)
        //        indices[indicesIndex++] = verticesIndex + i;

        //    for (int i = 0; i < newVertices.Count; i++)
        //        vertices[verticesIndex++] = newVertices[i];
        //}

        /// <summary>Slightly faster than by adding via List</summary>
        public void Add(V[] newVertices)
        {
            EnsureAdditionalArrayCapacity(newVertices.Length, newVertices.Length);

            for (int i = 0; i < newVertices.Length; i++)
                indices[indicesIndex++] = verticesIndex + i;

            Array.Copy(newVertices, 0, vertices, verticesIndex, newVertices.Length);
            verticesIndex += newVertices.Length;
        }

        //public void Add(IList<V> newVertices, IList<int> newIndices)
        //{
        //    EnsureAdditionalArrayCapacity(newVertices.Count, newIndices.Count);

        //    for (int i = 0; i < newIndices.Count; i++)
        //        indices[indicesIndex++] = verticesIndex + newIndices[i];

        //    for (int i = 0; i < newVertices.Count; i++)
        //        vertices[verticesIndex++] = newVertices[i];
        //}

        /// <summary>Slightly faster than by adding via List</summary>
        public void Add(V[] newVertices, int[] newIndices)
        {
            EnsureAdditionalArrayCapacity(newVertices.Length, newIndices.Length);

            Array.Copy(newIndices, 0, indices, indicesIndex, newIndices.Length);
            indicesIndex += newIndices.Length;

            Array.Copy(newVertices, 0, vertices, verticesIndex, newVertices.Length);
            verticesIndex += newVertices.Length;
        }

        internal void EnsureAdditionalArrayCapacity(int newVerticesToAdd, int newIndicesToAdd)
        {
            EnsureArrayCapacity(verticesIndex + newVerticesToAdd, indicesIndex + newIndicesToAdd);
        }

        private void EnsureArrayCapacity(int newVertexCount, int newIndexCount)
        {
            if (newVertexCount > vertices.Length)
            {
                newVertexCount = newVertexCount * 3 / 2; // grow by x1.5
                newVertexCount = (newVertexCount + 63) & (~63); // grow in chunks of 64.
                Array.Resize(ref vertices, newVertexCount);
            }
            if (newIndexCount > indices.Length)
            {
                newIndexCount = newIndexCount * 3 / 2; // grow by x1.5
                newIndexCount = (newIndexCount + 63) & (~63); // grow in chunks of 64.
                Array.Resize(ref indices, newIndexCount);
            }
        }

        public override void Begin()
        {
            verticesIndex = 0;
            indicesIndex = 0;
        }

        public override bool End(RenderSettings settings)
        {
            if (indicesIndex <= 0)
                return false;
            settings.Render(vertices, verticesIndex, indices, indicesIndex);
            return true;
        }
    }
}
