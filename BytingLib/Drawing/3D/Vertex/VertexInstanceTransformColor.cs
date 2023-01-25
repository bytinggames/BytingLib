using System.Runtime.InteropServices;

namespace BytingLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInstanceTransformColor : IVertexType
    {
        public Matrix Transform;
        public Color Color;

        public VertexInstanceTransformColor(Matrix transform, Color color)
        {
            Transform = transform;
            Color = color;
        }

        public static readonly VertexDeclaration vertexDeclaration;
        static VertexInstanceTransformColor()
        {
            VertexElement[] elements = new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(64, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            vertexDeclaration = declaration;
        }

        public VertexDeclaration VertexDeclaration => vertexDeclaration;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Transform.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                return hashCode;
            }
        }
    }
}
