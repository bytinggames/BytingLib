using BytingLib.DataTypes;
using System.Reflection;

namespace BytingLib.Serialization
{
    public class CtorData
    {
        public (byte ID, object Value)[] Data { get; set; }

        public CtorData(params (byte ID, object Value)[] parameters)
        {
            this.Data = parameters;
        }
    }

    public interface ICtorSerializable
    {
        CtorData Serialize();
    }

    public class CtorSerializer
    {
        private readonly Map<Type, int> typeIDs = new();
        private readonly Dictionary<Type, object> autoParams;

        public CtorSerializer(Dictionary<Type, int> typeIDs, object[] autoParams)
        {
            foreach (var item in typeIDs)
            {
                this.typeIDs.Add(item.Key, item.Value);
            }

            this.autoParams = new Dictionary<Type, object>();
            for (int i = 0; i < autoParams.Length; i++)
            {
                this.autoParams.Add(autoParams[i].GetType(), autoParams[i]);
            }
        }


        public void Serialize(BytingWriter bw, List<ICtorSerializable> entities)
        {
            using MemoryStream ms = new();
            BytingWriter temp = new(ms, bw.WriteTypes, bw.TypeToID);
            long tempPos = 0;

            bw.Write(entities.Count);
            for (int i = 0; i < entities.Count; i++)
            {
                bw.Write(typeIDs.Forward[entities[i].GetType()]);
                var dict = entities[i].Serialize().Data;
                bw.Write(dict.Length);
                foreach (var item in dict)
                {
                    bw.Write(item.ID);
                }
                foreach (var item in dict)
                {
                    temp.WriteObject(item.Value, item.Value.GetType());

                    int objectLength = (int)(temp.BaseStream.Position - tempPos);
                    bw.Write(objectLength);

                    // copy object from temp buffer to bw
                    temp.BaseStream.Position = tempPos;
                    temp.BaseStream.CopyTo(bw.BaseStream, objectLength);

                    tempPos = temp.BaseStream.Position;
                }
            }
        }

        public List<ICtorSerializable> Deserialize(BytingReader br)
        {
            List<ICtorSerializable> entities = new();

            int entityCount = br.ReadInt32();
            for (int entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                Type t = typeIDs.Backward[br.ReadInt32()];
                int outputCount = br.ReadInt32();
                byte[] outputIDs = new byte[outputCount];
                for (int j = 0; j < outputCount; j++)
                    outputIDs[j] = br.ReadByte();

                var ctors = t.GetConstructors();

                int cIndex;
                ParameterInfo[]? parameters = null;
                List<byte?> ctorParamIDs = new();
                for (cIndex = 0; cIndex < ctors.Length; cIndex++)
                {
                    parameters = ctors[cIndex].GetParameters();
                    int pIndex;
                    for (pIndex = 0; pIndex < parameters.Length; pIndex++)
                    {
                        CSAttribute? cs = parameters[pIndex].GetCustomAttribute<CSAttribute>();

                        if (cs == null)
                        {
                            if (autoParams.ContainsKey(parameters[pIndex].ParameterType))
                            {
                                ctorParamIDs.Add(null);
                                continue;
                            }
                            else
                                break;
                        }
                        
                        if (outputIDs.All(f => f != cs!.Id))
                            break;
                        ctorParamIDs.Add(cs!.Id);
                    }

                    if (pIndex == parameters.Length)
                        break;
                    ctorParamIDs = new List<byte?>();
                }
                if (cIndex == ctors.Length)
                    throw new Exception("no matching constructor found");

                ConstructorInfo ctor = ctors[cIndex];

                object?[] paramValues = new object?[parameters!.Length];
                for (int outputIndex = 0; outputIndex < outputCount; outputIndex++)
                {
                    int byteCount = br.ReadInt32();
                    int paramIndex = ctorParamIDs.IndexOf(outputIDs[outputIndex]);
                    object paramObj = br.ReadObject(parameters[paramIndex].ParameterType);
                    if (paramIndex != -1)
                    {
                        paramValues[paramIndex] = paramObj;
                    }
                }

                for (int inputIndex = 0; inputIndex < ctorParamIDs.Count; inputIndex++)
                {
                    if (ctorParamIDs[inputIndex] == null)
                    {
                        if (autoParams.TryGetValue(parameters[inputIndex].ParameterType, out object? obj))
                        {
                            paramValues[inputIndex] = obj;
                        }
                    }
                }


                entities.Add((ICtorSerializable)Activator.CreateInstance(t, paramValues)!);
            }

            return entities;
        }
    }

    public class CSAttribute : Attribute
    {
        public CSAttribute(byte id)
        {
            Id = id;
        }

        public byte Id { get; }
    }
}
