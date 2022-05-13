namespace BytingLib.Serialization
{
    internal class BytingWriterParent : BinaryWriter
    {
        Dictionary<Type, Action<BytingWriterParent, object>> writeTypes;
        private readonly Dictionary<Type, int> typeToID;

        public BytingWriterParent(Stream output, Dictionary<Type, Action<BytingWriterParent, object>> writeTypes, Dictionary<Type, int> typeToID) : base(output)
        {
            this.writeTypes = writeTypes;
            this.typeToID = typeToID;
        }

        public virtual void WriteObject(object obj, Type declarationType)
        {
            Type type = obj.GetType();
            if (!declarationType.IsSealed)
                Write(typeToID[type]);

            WriteObjectInternal(obj);
        }

        private void WriteObjectInternal(object obj)
        {
            Type type = obj.GetType();
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            writeTypes[type].Invoke(this, obj);
        }
    }
}
