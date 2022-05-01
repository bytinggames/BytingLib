using System.Runtime.InteropServices;

namespace BytingLib
{
    static class StructSerializer
    {
        /// <summary>returns null if stream couldn't read in the full structure.</summary>
        public static object? Read(Stream stream, Type t)
        {
            byte[] buffer = new byte[Marshal.SizeOf(t)];
            if (!stream.ReadFullBuffer(buffer))
                return null;

            return Read(buffer, t);
        }

        /// <summary>returns null if stream couldn't read in the full structure.</summary>
        public static object? Read(Stream stream, Type t, out byte[] buffer)
        {
            buffer = new byte[Marshal.SizeOf(t)];
            if (!stream.ReadFullBuffer(buffer))
                return null;

            return Read(buffer, t);
        }

        /// <summary>returns null if stream couldn't read in the full structure.</summary>
        public static object Read(byte[] buffer, Type t)
        {
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            object o = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), t)!;
            gcHandle.Free();
            return o;
        }

        public static byte[] Write(Stream stream, object o)
        {
            byte[] buffer = GetBytes(o);
            stream.Write(buffer, 0, buffer.Length);
            return buffer;
        }

        public static byte[] GetBytes(object o)
        {
            byte[] buffer = new byte[Marshal.SizeOf(o.GetType())];
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(o, gcHandle.AddrOfPinnedObject(), true);
            gcHandle.Free();

            return buffer;
        }

        // not tested: but probably slower
        //public static byte[] Serialize<T>(T data) where T : struct
        //{
        //    var formatter = new BinaryFormatter();
        //    var stream = new MemoryStream();
        //    formatter.Serialize(stream, data);
        //    return stream.ToArray();
        //}
        //public static T Deserialize<T>(byte[] array) where T : struct
        //{
        //    var stream = new MemoryStream(array);
        //    var formatter = new BinaryFormatter();
        //    return (T)formatter.Deserialize(stream);
        //}
    }
}
