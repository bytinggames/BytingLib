using Microsoft.Xna.Framework;
using System.Collections;

namespace BytingLib.Serialization
{
    class BinaryObjectReader
    {
        public static Dictionary<Type, ReadObj> ReadFunctions { get; } = new()
        {
            { typeof(int), (br, _, _) => br.ReadInt32() },
            { typeof(float), (br, _, _) => br.ReadSingle() },
            { typeof(Vector2), (br, _, _) => br.ReadVector2() },
            { typeof(List<>), (br, t, refs) => ReadList(br, t, refs) },
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
    }
}
