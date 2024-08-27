using System.Runtime.Serialization;

namespace BytingLib
{
    [Serializable]
    internal class VertexAttributeNotSupportedExeption : Exception
    {
        public VertexAttributeNotSupportedExeption()
        {
        }

        public VertexAttributeNotSupportedExeption(string? message) : base(message)
        {
        }

        public VertexAttributeNotSupportedExeption(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}