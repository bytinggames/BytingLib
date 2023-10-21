﻿using System.Collections;

namespace BytingLib.Serialization
{
    public static class BinaryObjectWriter
    {
        public static Dictionary<Type, Action<BytingWriter, object>> WriteFunctions { get; } = new()
        {
            { typeof(int), (bw, obj) => bw.Write((int)obj) },
            { typeof(float), (bw, obj) => bw.Write((float)obj) },
            { typeof(Vector2), (bw, obj) => bw.Write((Vector2)obj) },
            { typeof(List<>), WriteList },
            { typeof(Array), WriteArray },
            { typeof(string), (bw, obj) => bw.Write((string)obj) },
            { typeof(Color), (bw, obj) => bw.Write(((Color)obj).PackedValue) },
            { typeof(bool), (bw, obj) => bw.Write((bool)obj) },
        };

        private static void WriteList(BytingWriter bw, object obj)
        {
            IList list = (IList)obj;
            Type itemType = list.GetType().GenericTypeArguments[0];

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    bw.Write((byte)0); // null
                }
                else
                {
                    bw.Write((byte)1); // not null
                    bw.WriteObject(list[i]!, itemType);
                }
            }
        }

        private static void WriteArray(BytingWriter bw, object obj)
        {
            Array arr = (Array)obj;
            Type itemType = arr.GetType().GetElementType()!;

            bw.Write(arr.Length);

            for (int i = 0; i < arr.Length; i++)
            {
                object? item = arr.GetValue(i);
                if (item == null)
                {
                    bw.Write((byte)0); // null
                }
                else
                {
                    bw.Write((byte)1); // not null
                    bw.WriteObject(item, itemType);
                }
            }
        }
    }
}
