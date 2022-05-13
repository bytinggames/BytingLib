using System.Reflection;

namespace BytingLib.Serialization
{
    public class Serializer
    {
        private Dictionary<Type, ReadObj> readTypes;
        private Dictionary<Type, Action<BytingWriterParent, object>> writeTypes;

        const BindingFlags bindingFlagsDeclared = BindingFlags.Public
                        | BindingFlags.Instance
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

            InitializeForSerialization();
            InitializeForDeserialization();
        }

        void InitializeForSerialization()
        {
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
        }

        void InitializeForDeserialization()
        {
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

                        while (currentLevel >= 0)
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

                            currentLevel--;
                        }

                        return obj;
                    });
                }
            }

        }

        public void Serialize<T>(Stream stream, T obj)
        {
            if (obj == null)
                throw new ArgumentException(nameof(obj));

            using (BytingWriterParent bw = 
                References ? new BytingWriter(stream, writeTypes, typeIDs.IDs)
                : new BytingWriterParent(stream, writeTypes, typeIDs.IDs))
            {
                bw.WriteObject(obj, typeof(T));
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            using (BytingReader br = new BytingReader(stream, readTypes, typeIDs.Types, References))
            {
                return (T)br.ReadObject(typeof(T));
            }
        }
    }
}
