using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace BytingLib.Serialization
{
    class BinaryObjectWriter
    {
        public static Dictionary<Type, Action<BytingWriterParent, object>> WriteFunctions { get; } = new()
        {
            { typeof(int), (bw, obj) => bw.Write((int)obj) },
            { typeof(float), (bw, obj) => bw.Write((float)obj) },
            { typeof(Vector2), (bw, obj) => bw.Write((Vector2)obj) },
            { typeof(List<>), WriteList },
            { typeof(string), (bw, obj) => bw.Write((string)obj) },
            { typeof(Color), (bw, obj) => bw.Write(((Color)obj).PackedValue) },
            { typeof(bool), (bw, obj) => bw.Write((bool)obj) },
        };

        private static void WriteList(BytingWriterParent bw, object obj)
        {
            IList list = (IList)obj;
            Type itemType = list.GetType().GenericTypeArguments[0];

            bw.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    bw.Write((byte)0); // null
                else
                {
                    bw.Write((byte)1); // not null
                    bw.WriteObject(list[i]!, itemType);
                }
            }
        }
    }
}
