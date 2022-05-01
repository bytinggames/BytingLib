namespace BytingLib
{
    internal class ByteHelper
    {
        public static void SubtractBytes(byte[] subtractFrom, byte[] subtractBy, byte[] result)
        {
            for (int i = 0; i < subtractFrom.Length; i++)
            {
                result[i] = (byte)(subtractFrom[i] - subtractBy[i]);
            }
        }
        public static void AddBytes(byte[] arr1, byte[] arr2, byte[] result)
        {
            for (int i = 0; i < arr1.Length; i++)
            {
                result[i] = (byte)(arr1[i] + arr2[i]);
            }
        }
    }
}
