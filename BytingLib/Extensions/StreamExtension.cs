
namespace BytingLib
{
    public static class StreamExtension
    {
        public static bool ReadFullBuffer(this Stream stream, byte[] buffer)
        {
            int read = 0;
            while (read < buffer.Length)
            {
                int currentRead = stream.Read(buffer, read, buffer.Length);
                if (currentRead == 0)
                    return false;
                read += currentRead;
            }
            return true;
        }
    }
}
