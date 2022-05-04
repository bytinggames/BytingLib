using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public Creator(string defaultNamespace, Assembly[] assemblies = null, object[] _autoParameters = null, Type shortcutAttributeType = null)
        {
            this.defaultNamespace = defaultNamespace;
            this.assemblies = assemblies;

            if (assemblies == null)
                assemblies = new Assembly[] { Assembly.GetCallingAssembly() };

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

        public object CreateObject(ScriptReader reader)
        {
            object entity = CreateObject(reader, typeof(object));

            return entity;
        }

        public T CreateObject<T>(ScriptReader reader)
        {
            object entity = CreateObject(reader, typeof(T));
            return (T)entity;
        }

        public void ExecuteOnObject(object obj, ScriptReader reader)
        {
            Type type = obj.GetType();

            ExecuteOnObjectInner(obj, type, reader);
        }

        private void ExecuteOnObjectInner(object obj, Type type, ScriptReader reader)
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
        private object CreateObject(ScriptReader reader, Type objectBaseType)
        {
            string typeStr = reader.ReadToChar(open);

            Type type = null;
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

        private void SetPropertyMethodOrField(Type type, object obj, string setterName, ScriptReader reader)
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
        private object CreateObject(Type type, ScriptReader reader)
        {
            object[] args = GetParametersForConstructor(reader, type);

            return Activator.CreateInstance(type, args);
        }

        /// <summary>"ctorArg1,ctorArg2"</summary>
        private object[] GetParametersForConstructor(ScriptReader reader, Type constructorType)
        {
            var ctors = constructorType.GetConstructors();

            string[] split = GetParameterStrings(reader);

            ConstructorInfo ctorInfo = GetMatchingConstructor(ctors, split);
            if (ctorInfo == null)
                throw new Exception("no matching constructor not found");

            var parameterInfos = ctorInfo.GetParameters().ToArray();

            return GetParameters(split, parameterInfos.Select(f => f.ParameterType).ToArray());
        }

        private ConstructorInfo GetMatchingConstructor(ConstructorInfo[] ctors, string[] split)
        {
            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                int parametersForSplitArray = 0;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!AutoParameters.ContainsKey(parameters[i].ParameterType))
                        parametersForSplitArray++;
                }

                if (parametersForSplitArray == split.Length)
                    return ctor;
            }

            return null;
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
                if (AutoParameters.ContainsKey(expectedTypes[i]))
                    output[i] = AutoParameters[expectedTypes[i]];
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
            else if (argStr.Contains(open))
            {
                ScriptReader reader = new ScriptReader(argStr);
                return CreateObject(reader, expectedType);
            }
            else
                return Convert.ChangeType(argStr, expectedType, CultureInfo.InvariantCulture);
        }

        private string[] GetParameterStrings(ScriptReader reader)
        {
            List<string> splits = new List<string>();

            do
            {
                string para = reader.ReadToCharOrEndConsiderOpenCloseBraces(new char[] { close, parameterSeparator }, open, close);
                splits.Add(para);
            } while (reader.Peek(-1) == parameterSeparator);

            // clear list if paramters look like this: () <- empty
            if (splits.Count == 1 && splits[0] == "")
                splits.Clear();

            if (reader.Peek(-1) != close)
                throw new Exception($"close expected, but {reader.Peek(-1)} read instead");

            return splits.ToArray();
        }
    }
}
