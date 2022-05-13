
namespace BytingLib.Serialization
{
    public class TypeIDs
    {
        public Dictionary<Type, int> IDs { get; }

        public TypeIDs(Dictionary<Type, int> ids)
        {
            IDs = ids;

            foreach (var typeID in IDs)
            {
                Types.Add(typeID.Value, typeID.Key);

                TypeSerializers.Add(typeID.Key, new TypeSerializer(typeID.Key));
            }
        }

        public Dictionary<int, Type> Types { get; } = new();

        public Dictionary<Type, TypeSerializer> TypeSerializers { get; } = new();
    }
}
