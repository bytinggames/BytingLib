
namespace BytingLib.Serialization
{
    public delegate object ReadObj(BytingReader br, Type type, List<object>? refs);

    public class BytingReader : BinaryReader
    {
        Dictionary<Type, ReadObj> readTypes;
        private readonly Dictionary<int, Type> idToType;
        List<object>? refIDs;

        public BytingReader(Stream output, Dictionary<Type, ReadObj> readTypes, Dictionary<int, Type> idToType, bool references) : base(output)
        {
            this.readTypes = readTypes;
            this.idToType = idToType;

            if (references)
                refIDs = new();
        }

        private object ReadObjectForReal(Type declarationType)
        {
            if (!declarationType.IsSealed)
            {
                int id = ReadInt32();

                declarationType = idToType[id]; // swap declarationType to actual type
            }

            return readTypes
                [declarationType.IsGenericType ? declarationType.GetGenericTypeDefinition() : declarationType.IsEnum ? declarationType.GetEnumUnderlyingType() : declarationType]
                .Invoke(this, declarationType, refIDs);
        }

        public object ReadObject(Type declarationType)
        {
            if (refIDs == null || declarationType.IsValueType)
                return ReadObjectForReal(declarationType);

            byte oldReference = ReadByte();

            if (oldReference == 1)
            {
                int id = ReadInt32();
                return refIDs[id];
            }
            else
            {
                return ReadObjectForReal(declarationType);
            }
        }
    }
}
