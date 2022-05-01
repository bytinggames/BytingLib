using System.Runtime.Serialization;

namespace BytingLib
{
    [Serializable]
    internal class StructPlaybackException : Exception
    {
        public StructPlaybackException()
        {
        }

        public StructPlaybackException(string? message) : base(message)
        {
        }

        public StructPlaybackException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected StructPlaybackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}