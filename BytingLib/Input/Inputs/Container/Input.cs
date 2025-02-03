using System.Reflection;
using System.Text.RegularExpressions;

namespace BytingLib
{
    public class Input : IDisposable
    {
        protected IInputOutput[] outputs;
        protected readonly InputUpdater updater;

        private List<PropItem>? defaultSerialized;

        public Input(InputUpdater updater)
            :this(updater, true)
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
            for (int i = 0; i < outputs.Length; i++)
            {
                updater.RemoveOutput(outputs[i]);
            }
        }
        protected static Creator CreateCreator(Dictionary<Type, object>? autoParameters)
        {
            return new Creator("BytingLib", autoParameters ?? new(), null, typeof(InputShortcutAttribute))
            {
                ParameterSeparator = ','
            };
        }

        public string Serialize(Dictionary<Type, object>? autoParameters = null)
        {
            List<PropItem> serializedMe = SerializeInner(autoParameters);
            if (defaultSerialized == null)
            {
                Input defaultInstance = (Input)Activator.CreateInstance(GetType(), true)!;
                defaultSerialized = defaultInstance.SerializeInner(autoParameters);
            }

            for (int i = 0; i < serializedMe.Count; i++)
            {
                for (int j = 0; j < defaultSerialized.Count; j++)
                {
                    if (serializedMe[i].Prop == defaultSerialized[j].Prop)
                    {
                        if (serializedMe[i].Value == defaultSerialized[j].Value)
                        {
                            serializedMe.RemoveAt(i--);
                        }

                        break;
                    }
                }
            }

            return string.Join(",\n", serializedMe);
        }

        private List<PropItem> SerializeInner(Dictionary<Type, object>? autoParameters = null)
        {
            List<PropItem> output = new();

            Creator c = CreateCreator(autoParameters);

            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
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
                    IPointerValue? instance = (IPointerValue?)prop.GetValue(this);
                    if (instance == null)
                    {
                        continue;
                    }
                    var pointerVal = instance.GetPointerValue();
                    if (pointerVal == null)
                    {
                        continue;
                    }

                    string serialized = c.Serialize(pointerVal);
                    output.Add(new PropItem(prop.Name, serialized));
                }
                catch (Exception e)
                {
                    updater.OnException.Invoke(e);
                }
            }
            return output;
        }

        public void Override(string keymap, Dictionary<Type, object> autoParameters)
        {
            Creator c = CreateCreator(autoParameters);
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
                        object keyBindObject = c.CreateObject(reader2, pointer.GetDeclaredPointerValueType());
                        pointer.SetPointerValue(keyBindObject);
                    }
                }
                catch (Exception e)
                {
                    updater.OnException(e);
                }
            }
        }

        protected static BoolInput Ctrl() => new BoolCtrl();
        protected static BoolInput Shift() => new BoolShift();
        protected static BoolInput Alt() => new BoolAlt();
        protected static BoolInput And(params BoolInput[] inputs) => new BoolAnd(inputs);
        protected static BoolInput Or(params BoolInput[] inputs) => new BoolOr(inputs);
        protected static BoolInput Not(BoolInput input) => new BoolNot(input);

        private record PropItem(string Prop, string Value)
        {
            public override string ToString()
            {
                return $"{Prop}: {Value}";
            }
        }
    }
}
