using System.Runtime.InteropServices;

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

        public static T[] ByteArrayToStructArray<T>(byte[] bytes, int structSize) where T : struct
        {
            T[] t = new T[bytes.Length / structSize];
            IntPtr mPtr = Marshal.UnsafeAddrOfPinnedArrayElement(t, 0);
            Marshal.Copy(bytes, 0, mPtr, bytes.Length);
            return t;
        }

        public static T[] ByteArrayToStructArray<T>(byte[] bytes) where T : struct
            => ByteArrayToStructArray<T>(bytes, Marshal.SizeOf<T>());

        public static T ByteArrayToStruct<T>(byte[] bytes, int dataOffset, int structSize) where T : struct
        {
            var pData = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr addr = pData.AddrOfPinnedObject();
            addr = IntPtr.Add(addr, dataOffset);
            var result = Marshal.PtrToStructure<T>(addr);
            pData.Free();
            return result;
        }
        public static T ByteArrayToStruct<T>(byte[] bytes, int dataOffset) where T : struct
            => ByteArrayToStruct<T>(bytes, dataOffset, Marshal.SizeOf<T>());

        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

    }
}
