using System.Runtime.Serialization.Formatters.Binary;

namespace BytingLib
{
    public static class DeepCloner
    {
        // Deep clone
        [Obsolete("deprecated cause BinaryFormatter is deprecated")]
        public static T DeepClone<T>(T a) where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
