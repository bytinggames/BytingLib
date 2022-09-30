
namespace BytingLib.Serialization
{
    /// <summary>
    /// not recommended to use for big complex files. only [int.MaxValue] different reference types allowed
    /// Also probably not very performant?
    /// </summary>
    internal class BytingWriterRefs : BytingWriter
    {
        Dictionary<object, int> refIDs = new();
        int idCount = 0;

        public BytingWriterRefs(Stream output, Dictionary<Type, Action<BytingWriter, object>> writeTypes, Dictionary<Type, int> typeToID) : base(output, writeTypes, typeToID)
        {
        }

        public override void WriteObject(object obj, Type declarationType)
        {
            if (declarationType.IsValueType)
            {
                base.WriteObject(obj, declarationType);
                return;
            }

            if (refIDs.TryGetValue(obj, out int refID))
            {
                Write((byte)1); // old reference
                Write(refID);
            }
            else
            {
                Write((byte)0); // new reference
                refIDs.Add(obj, idCount++);
                base.WriteObject(obj, declarationType);
            }
        }
    }
}
