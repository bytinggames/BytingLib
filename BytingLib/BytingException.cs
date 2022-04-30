using System.Runtime.Serialization;

namespace BytingLib
{
    /// <summary>
    /// This exception is used for occurences that should never happen.
    /// If they still do, the code needs to be fixed.
    /// </summary>
    [Serializable]
    public class BytingException : Exception
    {
        public BytingException()
        {
        }

        public BytingException(string? message) : base(message + "\nThis shouldn't happen! Please contact bytinggames.")
        {
        }

        public BytingException(string? message, Exception? innerException) : base(message + "\nThis shouldn't happen! Please contact bytinggames.", innerException)
        {
        }

        protected BytingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}