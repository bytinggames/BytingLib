using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace BytingLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormal : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionNormal(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " Normal:" + this.Normal + "}}";
        }

        public static bool operator ==(VertexPositionNormal left, VertexPositionNormal right)
        {
            return left.Position == right.Position && left.Normal == right.Normal;
        }

        public static bool operator !=(VertexPositionNormal left, VertexPositionNormal right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPositionNormal)obj));
        }

        static VertexPositionNormal()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
