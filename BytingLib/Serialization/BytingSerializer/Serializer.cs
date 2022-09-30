using System.Reflection;

namespace BytingLib.Serialization
{
    public class Serializer
    {
        private readonly Dictionary<Type, ReadObj> readTypes;
        private readonly Dictionary<Type, Action<BytingWriter, object>> writeTypes;

        const BindingFlags bindingFlagsDeclared = TypeSerializer.BindingFlagsDeclaredAndInherited
                        | BindingFlags.DeclaredOnly;
        private readonly TypeIDs typeIDs;
        public bool References { get; set; }

        // [code] = [typeID] [object Data]
        // [object Data] = [inheritance layer count] [layer 1] [layer 2] ...
        // [layer X] = [variableCount] [variableID1] [variableValue1] [variableID2] [variableValue2] ...
        //      example: 2 0 Hello 1 World

        public Serializer(TypeIDs typeIDs, bool references)
        {
            this.typeIDs = typeIDs;
            References = references;

            #region initializer for serialization

            writeTypes = new();

            foreach (var item in BinaryObjectWriter.WriteFunctions)
                writeTypes.Add(item.Key, item.Value);

            foreach (var t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(f => f.GetTypes()))
            {
                if (typeIDs.IDs.ContainsKey(t)
                    && !writeTypes.ContainsKey(t))
                {
                    writeTypes.Add(t, (bw, obj) =>
                    {
                        List<List<PropInfoAndID>> levels = typeIDs.TypeSerializers[t].PropertyLevels;

                        bw.Write(levels.Count); // TODO layer count

                        for (int i = levels.Count - 1; i >= 0; i--)
                        {
                            bw.Write(levels[i].Count);
                            foreach (var prop in levels[i])
                            {
                                bw.Write(prop.ID);
                                object value = prop.Prop.GetValue(obj)!;
                                bw.WriteObject(value, prop.Prop.PropertyType);
                            }
                        }
                    });
                }
            }
            #endregion

            #region initialize for deserialization

            readTypes = new();

            Dictionary<(int, int), PropertyInfo> allProperties = new();

            foreach (var item in BinaryObjectReader.ReadFunctions)
                readTypes.Add(item.Key, item.Value);

            foreach (var t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(f => f.GetTypes()))
            {
                if (typeIDs.IDs.TryGetValue(t, out int id)
                    && !readTypes.ContainsKey(t))
                {
                    var props = t.GetProperties(bindingFlagsDeclared).Where(f => Attribute.IsDefined(f, typeof(BytingPropAttribute))).ToList();
                    foreach (var p in props)
                    {
                        BytingPropAttribute storeMemberAttribute = p.GetCustomAttribute<BytingPropAttribute>()!;
                        allProperties.Add((id, storeMemberAttribute.ID), p);
                    }

                    readTypes.Add(t, (br, type, refs) =>
                    {
                        object obj = Activator.CreateInstance(type)!;
                        refs?.Add(obj);

                        int levelCount = br.ReadInt32();

                        int currentLevel = levelCount - 1;

                        List<Type> typeHierarchy = new();
                        Type? currentLevelType = type;
                        for (int i = 0; i < levelCount; i++)
                        {
                            typeHierarchy.Add(currentLevelType!);
                            currentLevelType = currentLevelType!.BaseType;
                        }

                        for (; currentLevel >= 0; currentLevel--)
                        {
                            int propsCount = br.ReadInt32();
                            if (propsCount == 0)
                                continue;

                            int currentID = typeIDs.IDs[typeHierarchy[currentLevel]];
                            while (propsCount > 0)
                            {
                                int propID = br.ReadInt32();
                                    // get member by propID
                                    var prop = allProperties[(currentID, propID)];
                                prop.SetValue(obj, br.ReadObject(prop.PropertyType));

                                propsCount--;
                            }
                        }

                        return obj;
                    });
                }
            }
            #endregion
        }

        public void Serialize<T>(Stream stream, T? obj)
        {
            using (BytingWriter bw = 
                References ? new BytingWriterRefs(stream, writeTypes, typeIDs.IDs)
                : new BytingWriter(stream, writeTypes, typeIDs.IDs))
            {
                if (obj == null)
                    bw.Write((byte)0); // obj is null
                else
                {
                    bw.Write((byte)1); // obj is not null
                    bw.WriteObject(obj, typeof(T));
                }
            }
        }

        public T? Deserialize<T>(Stream stream)
        {
            using (BytingReader br = new BytingReader(stream, readTypes, typeIDs.Types, References))
            {
                if (br.ReadByte() == 0) // obj is null?
                    return default;
                return (T)br.ReadObject(typeof(T));
            }
        }
    }
}
