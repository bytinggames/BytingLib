namespace BytingLib.Serialization
{
    public class BytingWriter : BinaryWriter
    {
        public Dictionary<Type, Action<BytingWriter, object>> WriteTypes { get; }
        public Dictionary<Type, int> TypeToID { get; }

        public BytingWriter(Stream output, Dictionary<Type, Action<BytingWriter, object>> writeTypes, Dictionary<Type, int> typeToID) : base(output)
        {
            this.WriteTypes = writeTypes;
            this.TypeToID = typeToID;
        }

        public virtual void WriteObject(object obj, Type declarationType)
        {
            if (!declarationType.IsSealed)
            {
                Type type = obj.GetType();
                Write(TypeToID[type]);
            }

            WriteObjectInternal(obj);
        }

        private void WriteObjectInternal(object obj)
        {
            Type type = obj.GetType();
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            else if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }
            else if (type.IsArray)
            {
                type = typeof(Array);
            }
            
            WriteTypes[type].Invoke(this, obj);
        }
    }
}
