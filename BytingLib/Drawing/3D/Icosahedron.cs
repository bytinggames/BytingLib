namespace BytingLib
{
    public class Icosahedron
    {
        public static readonly Vector3[] VerticesSub;
        public static readonly int[][] IndicesSub;

        static Icosahedron()
        {
            var x = MakeIcosphere(1);
            VerticesSub = x.Item1.ToArray();
            IndicesSub = x.Item2.ToArray();
        }

        const float x = 0.525731112119133606f;
        const float z = 0.850650808352039932f;
        const float Null = 0f;

        public static readonly Vector3[] Vertices = new Vector3[]
        {
          new(-x,Null,z), new(x,Null,z), new(-x,Null,-z), new(x,Null,-z),
          new(Null,z,x), new(Null,z,-x), new(Null,-z,x), new(Null,-z,-x),
          new(z,x,Null), new(-z,x, Null), new(z,-x,Null), new(-z,-x, Null)
        };

        public static readonly int[][] Indices =
        {
          new int[]{0,4,1},new int[]{0,9,4},new int[]{9,5,4},new int[]{4,5,8},new int[]{4,8,1},
          new int[]{8,10,1},new int[]{8,3,10},new int[]{5,3,8},new int[]{5,2,3},new int[]{2,7,3},
          new int[]{7,10,3},new int[]{7,6,10},new int[]{7,11,6},new int[]{11,0,6},new int[]{0,1,6},
          new int[]{6,1,10},new int[]{9,0,11},new int[]{9,11,2},new int[]{9,2,5},new int[]{7,2,11}
        };

        static int VertexForEdge(Dictionary<(int, int), int> lookup, List<Vector3> vertices, int first, int second)
        {
            (int, int) key = first > second ? (second, first) : (first, second);

            if (!lookup.TryGetValue(key, out int index))
            {
                index = vertices.Count;
                lookup.Add(key, index);

                Vector3 edge0 = vertices[first];
                Vector3 edge1 = vertices[second];
                Vector3 point = Vector3.Normalize(edge0 + edge1);
                vertices.Add(point);
            }

            return index;
        }

        static List<int[]> Subdivide(List<Vector3> vertices, List<int[]> triangles)
        {
            Dictionary<(int, int), int> lookup = new();
            List<int[]> result = new();

            foreach (var each in triangles)
            {
                int[] mid = new int[3];
                for (int edge = 0; edge < 3; ++edge)
                {
                    mid[edge] = VertexForEdge(lookup, vertices,
                      each[edge], each[(edge + 1) % 3]);
                }

                result.Add(new int[] { each[0], mid[0], mid[2] });
                result.Add(new int[] { each[1], mid[1], mid[0] });
                result.Add(new int[] { each[2], mid[2], mid[1] });
                result.Add(new int[] { mid[0], mid[1], mid[2] });
            }

            return result;
        }

        static (List<Vector3>, List<int[]>) MakeIcosphere(int subdivisions)
        {
            List<Vector3> vertices = Vertices.ToList();
            List<int[]> triangles = Indices.ToList();

            for (int i = 0; i < subdivisions; ++i)
            {
                triangles = Subdivide(vertices, triangles);
            }

            return (vertices, triangles);
        }
    }
}
