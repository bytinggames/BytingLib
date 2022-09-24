
using System.Globalization;
using System.Reflection;

namespace BytingLib.Creation
{
    public class Creator
    {
        public const char open = '(';
        public const char close = ')';
        public const char setterSeparator = '_';
        public const char parameterSeparator = '|';

        public Dictionary<Type, object> AutoParameters { get; } = new Dictionary<Type, object>();

        Dictionary<string, Type> shortcuts = new Dictionary<string, Type>();

        private readonly string defaultNamespace;
        private readonly Assembly[] assemblies;
        private readonly Dictionary<Type, Func<string, object>> converters;

        public Creator(string defaultNamespace, Assembly[]? assemblies = null, object[]? _autoParameters = null, Type? shortcutAttributeType = null, Dictionary<Type, Func<string, object>>? converters = null)
        {
            assemblies ??= new Assembly[] { Assembly.GetCallingAssembly() };

            this.defaultNamespace = defaultNamespace;
            this.assemblies = assemblies;
            this.converters = converters ?? new Dictionary<Type, Func<string, object>>();

            AutoParameters.Add(GetType(), this);
            if (_autoParameters != null)
            {
                for (int i = 0; i < _autoParameters.Length; i++)
                {
                    AutoParameters.Add(_autoParameters[i].GetType(), _autoParameters[i]);
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
                if (c != setterSeparator)
                {
                    reader.Move(-1); // move back the wrongly read in char
                    return;
                }

                string setterName = reader.ReadToChar(open);

                SetPropertyMethodOrField(type, obj, setterName, reader);
            }
        }

        /// <summary>"Type(ctorArg1)(ctorArg2)_Prop(val)_Method(arg1)(arg2)"</summary>
        private object CreateObject(ScriptReaderLiteral reader, Type objectBaseType)
        {
            string typeStr = reader.ReadToChar(open);

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
                        break;
                }

                if (type == null)
                    throw new Exception("type " + fullTypeName + " not found in given assemblies");
            }

            if (!objectBaseType.IsAssignableFrom(type))
                throw new Exception("type " + nameof(type)  + " is not assignable to " + objectBaseType);
            
            object obj = CreateObject(type, reader);

            ExecuteOnObjectInner(obj, type, reader);

            return obj;
        }

        private void SetPropertyMethodOrField(Type type, object obj, string setterName, ScriptReaderLiteral reader)
        {
            var prop = type.GetProperty(setterName);
            if (prop != null)
            {
                prop.SetValue(obj, GetParameters(reader.ReadToCharOrEndConsiderOpenCloseBraces(close, open, close), prop.PropertyType));
            }
            else
            {
                var method = type.GetMethod(setterName);
                if (method != null)
                {
                    object[] args = GetParameters(GetParameterStrings(reader), method.GetParameters().Select(f => f.ParameterType).ToArray());
                    method.Invoke(obj, args);
                }
                else
                {
                    var field = type.GetField(setterName);

                    if (field != null)
                    {
                        field.SetValue(obj, GetParameters(reader.ReadToCharOrEndConsiderOpenCloseBraces(close, open, close), field.FieldType));
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
            object[] args = GetParametersForConstructor(reader, type);

            return Activator.CreateInstance(type, args)!;
        }

        /// <summary>"ctorArg1,ctorArg2"</summary>
        private object[] GetParametersForConstructor(ScriptReaderLiteral reader, Type constructorType)
        {
            var ctors = constructorType.GetConstructors();

            string[] split = GetParameterStrings(reader);

            ConstructorInfo? ctorInfo = GetMatchingConstructor(ctors, split);
            if (ctorInfo == null)
                throw new Exception("no matching constructor not found");

            var parameterInfos = ctorInfo.GetParameters().ToArray();

            return GetParameters(split, parameterInfos.Select(f => f.ParameterType).ToArray());
        }

        private ConstructorInfo? GetMatchingConstructor(ConstructorInfo[] ctors, string[] split)
        {
            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                int parametersForSplitArray = 0;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!TryGetAutoParameter(parameters[i].ParameterType, out _))
                        parametersForSplitArray++;
                }

                if (parametersForSplitArray == split.Length)
                    return ctor;
            }

            return null;
        }

        private bool TryGetAutoParameter(Type parameterType, out object? obj)
        {
            if (AutoParameters.TryGetValue(parameterType, out obj))
                return true;

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
        private object[] GetParameters(string[] split, Type[] expectedTypes)
        {
            if (split == null)
                split = new string[0];

            object[] output = new object[expectedTypes.Length];

            int splitIndex = 0;
            for (int i = 0; i < expectedTypes.Length; i++)
            {
                if (TryGetAutoParameter(expectedTypes[i], out object? obj))
                    output[i] = obj!;
                else
                    output[i] = GetParameters(split[splitIndex++], expectedTypes[i]);
            }

            return output;
        }

        /// <summary>"ctorArg1"</summary>
        private object GetParameters(string argStr, Type expectedType)
        {
            if (expectedType == typeof(string))
                return argStr;
            else if (converters.TryGetValue(expectedType, out var converter))
            {
                return converter.Invoke(argStr);
            }
            else if (argStr.Contains(open))
            {
                ScriptReaderLiteral reader = new ScriptReaderLiteral(argStr);
                return CreateObject(reader, expectedType);
            }
            else
                return Convert.ChangeType(argStr, expectedType, CultureInfo.InvariantCulture);
        }

        private string[] GetParameterStrings(ScriptReaderLiteral reader)
        {
            List<string> splits = new List<string>();

            reader.RemoveLiteralCharEnabled = false;
            do
            {
                string para = reader.ReadToCharOrEndConsiderOpenCloseBraces(new char[] { close, parameterSeparator }, open, close);
                splits.Add(para);
            } while (!reader.EndOfString() && reader.Peek(-1) == parameterSeparator);
            reader.RemoveLiteralCharEnabled = true;

            // clear list if paramters look like this: () <- empty
            if (splits.Count == 1 && splits[0] == "")
                splits.Clear();

            if (reader.Peek(-1) != close)
                throw new Exception($"close expected, but {reader.Peek(-1)} read instead");

            return splits.ToArray();
        }
    }
}
