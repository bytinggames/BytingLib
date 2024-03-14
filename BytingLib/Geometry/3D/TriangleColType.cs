namespace BytingLib
{
    public enum TriangleColType
    {
        Unknown = -2,
        None = -1,
        /// <summary>might not be the vertex at index 0. Didn't test this yet.</summary>
        Vertex0 = 0,
        /// <summary>might not be the vertex at index 1. Didn't test this yet.</summary>
        Vertex1 = 1,
        /// <summary>might not be the vertex at index 2. Didn't test this yet.</summary>
        Vertex2 = 2,
        /// <summary>might not be the vertex at index 0. Didn't test this yet.</summary>
        Edge0 = 3,
        /// <summary>might not be the vertex at index 1. Didn't test this yet.</summary>
        Edge1 = 4,
        /// <summary>might not be the vertex at index 2. Didn't test this yet.</summary>
        Edge2 = 5,
        Face = 6,
    }

    public static class TriangleColTypeExtension
    {
        public static bool IsVertex(this TriangleColType t)
        {
            return t >= TriangleColType.Vertex0 && t <= TriangleColType.Vertex2;
        }
        public static bool IsEdge(this TriangleColType t)
        {
            return t >= TriangleColType.Edge0 && t <= TriangleColType.Edge2;
        }
    }
}
