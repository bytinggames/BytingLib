using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexInstanceTransform : IVertexType
    {
        public Matrix Transform;

        public VertexInstanceTransform(Matrix transform)
        {
            Transform = transform;
        }

        public static readonly VertexDeclaration vertexDeclaration;
        static VertexInstanceTransform()
        {
            VertexElement[] elements = new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
            };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            vertexDeclaration = declaration;
        }

        public VertexDeclaration VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Transform.GetHashCode();
            }
        }
    }
}
