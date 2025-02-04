using System.Reflection;
using System.Text.RegularExpressions;

namespace BytingLib
{
    public class Input : IDisposable
    {
        protected IInputOutput[] outputs;
        protected readonly InputUpdater updater;

        private List<PropItem>? _defaultSerialized;
        private List<PropItem> DefaultSerialized
        {
            get
            {
                if (_defaultSerialized == null)
                {
                    _defaultSerialized = InitializeDefaultSerialized();
                }

                return _defaultSerialized;
            }
        }

        static Dictionary<Type, Input> defaults = new();

        public Input(InputUpdater updater)
            : this(updater, true)
        {
        }

        /// <summary>
        /// If you set initializeOutputs to false, you must call InitializeOutputs() manually.
        /// Best practice is at the end of the constructor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Input(InputUpdater updater, bool initializeOutputs = true)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.updater = updater;
            if (initializeOutputs)
            {
                InitializeOutputs();
            }
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // used for default serialization
        protected Input() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected void InitializeOutputs()
        {
            outputs = GetOutputs();
        }

        private IInputOutput[] GetOutputs()
        {
            Dictionary<IInputOutput, IInputOutput[]> outputsDict = new();

            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                if (!prop.PropertyType.IsAssignableTo(typeof(IInputOutput)))
                {
                    continue;
                }
                IInputOutput? output = (IInputOutput?)prop.GetValue(this);
                if (output == null)
                {
                    continue;
                }
                if (outputsDict.ContainsKey(output))
                {
                    continue;
                }
                outputsDict.Add(output, output.GetDependencies().ToArray());
            }

            HashSet<IInputOutput> alreadyAdded = new();
            List<IInputOutput> orderedOutputs = new();
            // order outputs by dependencies
            foreach (var o in outputsDict)
            {
                GetLeaves(o.Key, o.Value);
            }

            void GetLeaves(IInputOutput current, IInputOutput[] dependencies)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (alreadyAdded.Contains(dependencies[i]))
                    {
                        continue;
                    }
                    if (outputsDict.TryGetValue(dependencies[i], out IInputOutput[]? nextDependencies))
                    {
                        GetLeaves(dependencies[i], nextDependencies);
                    }
                    else
                    {
                        // dependency doesn't exist locally, so this output is called after the dependency
                    }
                }
                alreadyAdded.Add(current);
                orderedOutputs.Add(current);
            }

            for (int i = 0; i < orderedOutputs.Count; i++)
            {
                // add to updater
                // if not added, that means it already has been added. So it's not unique and we don't need to keep track of it
                if (!updater.AddOutput(orderedOutputs[i]))
                {
                    orderedOutputs.RemoveAt(i--);
                }
            }

            return orderedOutputs.ToArray();
        }


        public void Dispose()
        {
            if (outputs == null || updater == null)
            {
                return;
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                updater.RemoveOutput(outputs[i]);
            }
        }

        public static Creator Creator { get; } =
            new Creator("BytingLib", new(), null, typeof(InputShortcutAttribute))
            {
                ParameterSeparator = ','
            };

        private List<PropItem> InitializeDefaultSerialized()
        {
            Input defaultInstance = GetDefault();
            return defaultInstance.SerializeInner(null);
        }

        public static string SerializeToString(List<PropItem> props)
        {
            return string.Join(",\n", props);
        }
        public void Serialize(List<PropItem> serialized)
        {
            List<PropItem> serializedMe = SerializeInner(serialized);

            for (int i = 0; i < serializedMe.Count; i++)
            {
                for (int j = 0; j < DefaultSerialized.Count; j++)
                {
                    if (serializedMe[i].Prop == DefaultSerialized[j].Prop)
                    {
                        if (serializedMe[i].Value == DefaultSerialized[j].Value)
                        {
                            serializedMe.RemoveAt(i--);
                        }

                        break;
                    }
                }
            }

            serialized.AddRange(serializedMe);
        }

        private IEnumerable<PropInstance> GetPointers()
        {
            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                IPointerValue? instance = null;
                try
                {
                    var ignoreAttr = prop.GetCustomAttribute<CreatorIgnoreAttribute>(true);
                    if (ignoreAttr != null)
                    {
                        continue;
                    }
                    if (!prop.PropertyType.IsAssignableTo(typeof(IPointerValue)))
                    {
                        continue;
                    }
                    instance = (IPointerValue?)prop.GetValue(this);
                    if (instance == null)
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    updater.OnException.Invoke(e);
                }
                if (instance != null)
                {
                    yield return new(prop.Name, instance);
                }
            }
        }

        private List<PropItem> SerializeInner(List<PropItem>? alreadySerialized)
        {
            List<PropItem> output = new();

            var props = GetType().GetProperties();

            foreach (PropInstance prop in GetPointers())
            {
                try
                {
                    if (alreadySerialized != null && alreadySerialized.Any(f => f.Prop == prop.Name))
                    {
                        continue;
                    }
                    var pointerVal = prop.Pointer.GetPointerValue();
                    if (pointerVal == null)
                    {
                        continue;
                    }

                    string serialized = Creator.Serialize(pointerVal);
                    output.Add(new PropItem(prop.Name, serialized));
                }
                catch (Exception e)
                {
                    updater.OnException.Invoke(e);
                }
            }
            return output;
        }

        public void Override(string keymap)
        {
            keymap = Regex.Replace(keymap, @"\s+", "");

            ScriptReaderLiteral reader = new(keymap);
            Type type = GetType();
            while (!reader.EndOfString())
            {
                try
                {
                    string propertyName = reader.ReadToCharOrEnd(out char? foundChar, ':');
                    if (foundChar == null)
                    {
                        break;
                    }
                    string propertyCode = reader.ReadToCharOrEndConsiderOpenCloseBraces(',', '(', ')');
                    var prop = type.GetProperty(propertyName);
                    if (prop == null)
                    {
                        continue;
                    }
                    var propObject = prop.GetValue(this);
                    if (propObject is IPointerValue pointer)
                    {
                        //var valProp = prop.PropertyType.GetProperty("Value", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        ScriptReaderLiteral reader2 = new(propertyCode);
                        object keyBindObject = Creator.CreateObject(reader2, pointer.GetDeclaredPointerValueType());
                        pointer.SetPointerValue(keyBindObject);
                    }
                }
                catch (Exception e)
                {
                    updater.OnException(e);
                }
            }
        }

        public static T? CreateDefault<T>() where T : Input
        {
            return (T?)Activator.CreateInstance(typeof(T), true);
        }

        public Input GetDefault()
        {
            Type t = GetType();
            Input? input;
            if (!defaults.TryGetValue(t, out input))
            {
                input = (Input)Activator.CreateInstance(GetType(), true)!;
                defaults.Add(t, input);
            }
            return input;
        }

        protected static BoolInput Ctrl() => new BoolCtrl();
        protected static BoolInput Shift() => new BoolShift();
        protected static BoolInput Alt() => new BoolAlt();
        protected static BoolInput And(params BoolInput[] inputs) => new BoolAnd(inputs);
        protected static BoolInput Or(params BoolInput[] inputs) => new BoolOr(inputs);
        protected static BoolInput Not(BoolInput input) => new BoolNot(input);

        public record PropItem(string Prop, string Value)
        {
            public override string ToString()
            {
                return $"{Prop}:{Value}";
            }
        }

        public record PropInstance(string Name, IPointerValue Pointer);

        public IEnumerable<PropInstance> GetCustomizableProperties() => GetPointers();
    }
}
