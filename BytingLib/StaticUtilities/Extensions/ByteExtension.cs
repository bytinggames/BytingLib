namespace BytingLib
{
    public static class ByteExtension
    {
        public static void SetBit(ref byte value, int index)
        {
            value |= (byte)(1 << index);
        }

        public static void UnsetBit(ref byte value, int index)
        {
            value &= (byte)~(1 << index);
        }

        public static bool IsBitSet(byte value, int index)
        {
            return (value & (1 << index)) != 0;
        }
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
