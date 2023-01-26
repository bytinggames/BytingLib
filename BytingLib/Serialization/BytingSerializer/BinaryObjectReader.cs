using System.Collections;

namespace BytingLib.Serialization
{
    public static class BinaryObjectReader
    {
        public static Dictionary<Type, ReadObj> ReadFunctions { get; } = new()
        {
            { typeof(int), (br, _, _) => br.ReadInt32() },
            { typeof(float), (br, _, _) => br.ReadSingle() },
            { typeof(Vector2), (br, _, _) => br.ReadVector2() },
            { typeof(List<>),ReadList },
            { typeof(Array), ReadArray },
            { typeof(string), (br, _, _) => br.ReadString() },
            { typeof(Color), (br, _, _) => new Color(br.ReadUInt32()) },
            { typeof(bool), (br, _, _) => br.ReadBoolean() },
        };

        private static IList ReadList(BytingReader br, Type listType, List<object>? refs)
        {
            Type itemType = listType.GenericTypeArguments[0];
            IList list = (IList)Activator.CreateInstance(listType)!;
            refs?.Add(list);

            for (int count = br.ReadInt32(); count > 0; count--)
            {
                if (br.ReadByte() == 0)
                    continue;

                list.Add(br.ReadObject(itemType));
            }
            return list;
        }

        /// <summary>ONUSE: unit test</summary>
        private static Array ReadArray(BytingReader br, Type arrType, List<object>? refs)
        {
            Type itemType = arrType.GetElementType()!;

            int count = br.ReadInt32();

            Array arr = Array.CreateInstance(itemType, count);
            refs?.Add(arr);

            for (int i = 0; i < count; i++)
            {
                if (br.ReadByte() == 0)
                    continue;

                arr.SetValue(br.ReadObject(itemType), i);
            }

            return arr;
        }
    }
}
