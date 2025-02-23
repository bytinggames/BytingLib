using System.Globalization;
using System.Reflection;

namespace BytingLib
{
    public class Creator
    {
        public char Open { get; set; } = '(';
        public char Close { get; set; } = ')';
        public char SetterSeparator { get; set; } = '_';
        public char ParameterSeparator { get; set; } = '|';

        public Dictionary<Type, object> AutoParameters { get; } = new Dictionary<Type, object>();

        Dictionary<string, Type> shortcuts = new Dictionary<string, Type>();

        private readonly string defaultNamespace;
        private readonly Assembly[] assemblies;
        private readonly Type? shortcutAttributeType;
        private readonly Dictionary<Type, Func<string, object>> converters;

        public Creator(string defaultNamespace, Assembly[]? assemblies = null, object[]? _autoParameters = null, Type? shortcutAttributeType = null, Dictionary<Type, Func<string, object>>? converters = null)
            :this(defaultNamespace, ToDictionary(_autoParameters), assemblies, shortcutAttributeType, converters)
        {
        }

        public Creator(string defaultNamespace, Dictionary<Type, object> _autoParameters, Assembly[]? assemblies = null, Type? shortcutAttributeType = null, Dictionary<Type, Func<string, object>>? converters = null)
        {
            assemblies ??= [Assembly.GetCallingAssembly()];

            this.defaultNamespace = defaultNamespace;
            this.assemblies = assemblies;
            this.shortcutAttributeType = shortcutAttributeType;
            this.converters = converters ?? new Dictionary<Type, Func<string, object>>();

            AutoParameters.Add(GetType(), this);
            if (_autoParameters != null)
            {
                foreach (var parameter in _autoParameters)
                {
                    AutoParameters.Add(parameter.Key, parameter.Value);
                }
            }

            if (shortcutAttributeType != null)
            {
                foreach (var assembly in assemblies)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        var attributes = type.GetCustomAttributes(shortcutAttributeType, false).Cast<CreatorShortcutAttribute>();
                        foreach (var attr in attributes)
                        {
                            shortcuts.Add(attr.ShortcutName, type);
                        }
                    }
                }
            }
        }

        private static Dictionary<Type, object> ToDictionary(object[]? autoParameters)
        {
            if (autoParameters == null || autoParameters.Length == 0)
            {
                return new();
            }
            Dictionary<Type, object> dict = new();

            for (int i = 0; i < autoParameters.Length; i++)
            {
                dict.Add(autoParameters[i].GetType(), autoParameters[i]);
            }
            return dict;
        }

        public void AddShortcut(string name, Type type)
        {
            shortcuts.Add(name, type);
        }

        public object CreateObject(ScriptReaderLiteral reader)
        {
            object entity = CreateObject(reader, typeof(object));

            return entity;
        }

        public T CreateObject<T>(ScriptReaderLiteral reader)
        {
            object entity = CreateObject(reader, typeof(T));
            return (T)entity;
        }

        public void ExecuteOnObject(object obj, ScriptReaderLiteral reader)
        {
            Type type = obj.GetType();

            ExecuteOnObjectInner(obj, type, reader);
        }

        private void ExecuteOnObjectInner(object obj, Type type, ScriptReaderLiteral reader)
        {
            char? c;

            while ((c = reader.ReadChar()).HasValue)
            {
                if (c != SetterSeparator)
                {
                    reader.Move(-1); // move back the wrongly read in char
                    return;
                }

                char? peek = reader.Peek();
                if (peek == null)
                {
                    return;
                }

                if (peek >= '0' && peek <= '9') // if it starts with a number, it can't be a member or method, so stop.
                {
                    return;
                }

                string setterName = reader.ReadToChar(Open);

                SetPropertyMethodOrField(type, obj, setterName, reader);
            }
        }

        /// <summary>"Type(ctorArg1)(ctorArg2)_Prop(val)_Method(arg1)(arg2)"</summary>
        public object CreateObject(ScriptReaderLiteral reader, Type objectBaseType)
        {
            string typeStr = reader.ReadToCharOrEnd(out char? foundChar, Open);

            if (foundChar == null)
            {
                if (objectBaseType == typeof(object))
                {
                    throw new Exception("braces () missing or provide a objectBaseType");
                }
                // no ()
                // try to convert directly to objectBaseType
                return Convert.ChangeType(typeStr, objectBaseType, CultureInfo.InvariantCulture);
            }

            Type? type = null;
            if (shortcuts.ContainsKey(typeStr))
            {
                type = shortcuts[typeStr];
            }
            else
            {
                string fullTypeName = defaultNamespace + "." + typeStr;

                for (int i = 0; i < assemblies.Length; i++)
                {
                    type = assemblies[i].GetType(fullTypeName);
                    if (type != null)
                    {
                        break;
                    }
                }

                if (type == null)
                {
                    throw new Exception("type " + fullTypeName + " not found in given assemblies");
                }
            }

            if (!objectBaseType.IsAssignableFrom(type))
            {
                throw new Exception("type " + type  + " is not assignable to " + objectBaseType);
            }

            object obj = CreateObject(type, reader);

            ExecuteOnObjectInner(obj, type, reader);

            return obj;
        }

        private void SetPropertyMethodOrField(Type type, object obj, string setterName, ScriptReaderLiteral reader)
        {
            var prop = type.GetProperty(setterName);
            if (prop != null)
            {
                prop.SetValue(obj, GetParameter(reader.ReadToCharOrEndConsiderOpenCloseBraces(Close, Open, Close), prop.PropertyType));
            }
            else
            {
                var method = type.GetMethod(setterName);
                if (method != null)
                {
                    object[] args = GetParameters(GetParameterStrings(reader), method.GetParameters().Select(f => f.ParameterType).ToArray(), false /* no params support for methods for now */);
                    method.Invoke(obj, args);
                }
                else
                {
                    var field = type.GetField(setterName);

                    if (field != null)
                    {
                        field.SetValue(obj, GetParameter(reader.ReadToCharOrEndConsiderOpenCloseBraces(Close, Open, Close), field.FieldType));
                    }
                    else
                    {
                        throw new Exception("couldn't find property, method or field " + setterName + " of type " + type.FullName);
                    }
                }
            }
        }

        /// <summary>"ctorArg1,ctorArg2"</summary>
        private object CreateObject(Type type, ScriptReaderLiteral reader)
        {
            object?[] args = GetParametersForConstructor(reader, type);
            return Activator.CreateInstance(type,
                BindingFlags.CreateInstance |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.OptionalParamBinding,
                null,
                args,
                CultureInfo.InvariantCulture)!;
        }

        /// <summary>"ctorArg1,ctorArg2"</summary>
        private object[] GetParametersForConstructor(ScriptReaderLiteral reader, Type constructorType)
        {
            var ctors = constructorType.GetConstructors();

            string[] split = GetParameterStrings(reader);
            ConstructorInfo? ctorInfo = GetMatchingConstructor(ctors, split, out bool lastParameterIsParamsAttribute);
            if (ctorInfo == null)
            {
                throw new Exception("no matching constructor found for type " + constructorType.Name);
            }

            var parameterInfos = ctorInfo.GetParameters().ToArray();

            return GetParameters(split, parameterInfos.Select(f => f.ParameterType).ToArray(), lastParameterIsParamsAttribute);
        }

        private ConstructorInfo? GetMatchingConstructor(ConstructorInfo[] ctors, string[] split, out bool lastParameterIsParamsAttribute)
        {
            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                int parametersForSplitArray = 0;
                int parametersForSplitArrayOptional = 0;
                lastParameterIsParamsAttribute = false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!TryGetAutoParameter(parameters[i].ParameterType, out _))
                    {
                        if (i == parameters.Length - 1) // last parameter
                        {
                            var paramAttribute = parameters[i].GetCustomAttribute<ParamArrayAttribute>(false);
                            if (paramAttribute != null)
                            {
                                lastParameterIsParamsAttribute = true;
                                break;
                            }
                        }

                        if (parameters[i].IsOptional)
                        {
                            parametersForSplitArrayOptional++;
                        }
                        else
                        {
                            parametersForSplitArray++;
                        }
                    }
                }

                if (split.Length >= parametersForSplitArray && split.Length <= parametersForSplitArray + parametersForSplitArrayOptional
                    || lastParameterIsParamsAttribute && split.Length >= parametersForSplitArray)
                {
                    return ctor;
                }
            }

            lastParameterIsParamsAttribute = false;
            return null;
        }

        private bool TryGetAutoParameter(Type parameterType, out object? obj)
        {
            if (AutoParameters.TryGetValue(parameterType, out obj))
            {
                return true;
            }

            if (parameterType.IsInterface)
            {
                var first = AutoParameters.FirstOrDefault(f => parameterType.IsAssignableFrom(f.Key));
                if (first.Key != default)
                {
                    obj = first.Value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>{"ctorArg1", "ctorArg2"}</summary>
        private object[] GetParameters(string[] split, Type[] expectedTypes, bool lastParameterIsParamsAttribute)
        {
            if (split == null)
            {
                split = [];
            }

            object[] output = new object[expectedTypes.Length];

            int splitIndex = 0;
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                if (TryGetAutoParameter(expectedTypes[i], out object? obj))
                {
                    output[i] = obj!;
                }
                else
                {
                    if (splitIndex >= split.Length)
                    {
                        // no values left. Skip the optional parameters
                        output[i] = Type.Missing;
                    }
                    else
                    {
                        if (i == expectedTypes.Length - 1 && lastParameterIsParamsAttribute)
                        {
                            Type elementType = expectedTypes[i].GetElementType()!;
                            Array arr = Array.CreateInstance(elementType, split.Length - splitIndex);
                            for (int j = 0; j < arr.Length; j++)
                            {
                                arr.SetValue(GetParameter(split[splitIndex++], elementType), j); // TODO: array to non-array
                            }
                            output[i] = arr;
                        }
                        else
                        {
                            output[i] = GetParameter(split[splitIndex++], expectedTypes[i]);
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>"ctorArg1"</summary>
        private object GetParameter(string argStr, Type expectedType)
        {
            Type? nullableUnderlyingType;
            if (expectedType == typeof(string))
            {
                return argStr;
            }
            else if (expectedType.IsEnum)
            {
                if (Enum.TryParse(expectedType, argStr, out object? result) && result != null)
                {
                    return result;
                }

                throw new ArgumentException("couldn't parse '" + argStr + "' as enum type " + expectedType);
            }
            else if (converters.TryGetValue(expectedType, out var converter))
            {
                return converter.Invoke(argStr);
            }
            else if (argStr.Contains(Open))
            {
                ScriptReaderLiteral reader = new ScriptReaderLiteral(argStr);
                return CreateObject(reader, expectedType);
            }
            else if ((nullableUnderlyingType = Nullable.GetUnderlyingType(expectedType)) != null)
            {
                if (argStr == "null")
                {
                    return Activator.CreateInstance(expectedType)!; // Nullable with null as default
                }
                return Convert.ChangeType(argStr, nullableUnderlyingType, CultureInfo.InvariantCulture);
            }
            else
            {
                return Convert.ChangeType(argStr, expectedType, CultureInfo.InvariantCulture);
            }
        }

        private string[] GetParameterStrings(ScriptReaderLiteral reader)
        {
            List<string> splits = new List<string>();

            //reader.RemoveLiteralCharEnabled = false;
            do
            {
                string para = reader.ReadToCharOrEndConsiderOpenCloseBraces([Close, ParameterSeparator], Open, Close);
                splits.Add(para);
            } while (!reader.EndOfString() && reader.Peek(-1) == ParameterSeparator);
            //reader.RemoveLiteralCharEnabled = true;

            // clear list if paramters look like this: () <- empty
            if (splits.Count == 1 && splits[0] == "")
            {
                splits.Clear();
            }

            if (reader.Peek(-1) != Close)
            {
                throw new Exception($"close char '{Close}' expected, but {reader.Peek(-1)} read instead: {reader.GetString()} at position {reader.Position - 1}");
            }

            return splits.ToArray();
        }

        public void ReplaceAutoParameter(Type type, object value)
        {
            AutoParameters.Remove(type);
            AutoParameters.Add(type, value);
        }

        /// <summary>
        /// Experimental. only culture independent for float and double. Might break on some culture dependant stuff.
        /// </summary>
        public string Serialize(object? obj)
        {
            if (obj == null)
            {
                return "null";
            }

            Type type = obj.GetType();

            if (type.IsEnum || type.IsValueType)
            {
                if (obj is float f)
                {
                    return f.ToString(CultureInfo.InvariantCulture);
                }
                else if (obj is double d)
                {
                    return d.ToString(CultureInfo.InvariantCulture);
                }
                return obj.ToString() ?? "";
            }

            string? className = null;

            if (shortcutAttributeType != null)
            {
                CreatorShortcutAttribute? shortcutAttribute = (CreatorShortcutAttribute?)type.GetCustomAttribute(shortcutAttributeType);
                if (shortcutAttribute != null)
                {
                    className = shortcutAttribute.ShortcutName;
                }
            }
            if (className == null)
            {
                className = type.Name;
            }
            string str = className + Open;

            var ctors = obj.GetType().GetConstructors();
            if (ctors.Length > 1)
            {
                throw new Exception("only one constructor is currently supported");
            }
            var ctor = ctors[0];
            var parameters = ctor.GetParameters();
            int addedParameterCount = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (addedParameterCount > 0)
                {
                    str += ParameterSeparator;
                }

                var p = parameters[i];

                if (AutoParameters.ContainsKey(p.ParameterType))
                {
                    continue;
                }

                if (p.Name == null)
                {
                    throw new Exception($"ctor parameter {i} of {type} has no name");
                }
                var prop = type.GetProperty(p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
                object? instance;
                if (prop != null)
                {
                    instance = prop.GetValue(obj) ?? throw new Exception($"couldn't get value from prop {p.Name} of {type}");
                }
                else
                {
                    string name = p.Name;
                    var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                    if (field == null)
                    {
                        name = $"<{name}>P"; // this may only be used for class Name(int field); fields?
                        field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
                    }

                    if (field != null)
                    {
                        instance = field.GetValue(obj);// ?? throw new Exception($"couldn't get value from field {name} of {type}");
                    }
                    else
                    {
                        throw new Exception($"prop and field {p.Name} of {type} doesn't exist");
                    }
                }

                var paramAttribute = p.GetCustomAttribute<ParamArrayAttribute>(false);
                if (paramAttribute == null || instance == null)
                {
                    str += Serialize(instance);
                }
                else
                {
                    // iterate over array and serialize each instances
                    Array arr = (Array)instance;
                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (j > 0)
                        {
                            str += ParameterSeparator;
                        }
                        var val = arr.GetValue(j);
                        if (val == null)
                        {
                            str += "null";
                        }
                        else
                        {
                            str += Serialize(val);
                        }
                    }
                }


                addedParameterCount++;
            }

            str += Close;
            return str;
        }
    }
}
