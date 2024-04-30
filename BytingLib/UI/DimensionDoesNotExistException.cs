using System.Runtime.Serialization;

namespace BytingLib.UI
{
    [Serializable]
    internal class DimensionDoesNotExistException : Exception
    {
        public DimensionDoesNotExistException()
        {
        }

        public DimensionDoesNotExistException(int d)
            : base("Dimension " + d + " does not exist")
        {
        }

        public DimensionDoesNotExistException(string? message) : base(message)
        {
        }

        public DimensionDoesNotExistException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}