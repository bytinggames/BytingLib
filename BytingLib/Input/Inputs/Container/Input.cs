using System.Text.RegularExpressions;

namespace BytingLib
{
    public class Input : IDisposable
    {
        protected IInputOutput[] outputs;
        protected readonly InputUpdater updater;

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

        protected void InitializeOutputs()
        {
            outputs = GetOutputs();
        }

        private IInputOutput[] GetOutputs()
        {
            List<IInputOutput> outputsList = new();

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

                // add to updater
                // if not added, that means it already has been added. So it's not unique and we don't need to keep track of it
                if (!updater.AddOutput(output))
                {
                    continue;
                }
                outputsList.Add(output);
            }

            return outputsList.ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                updater.RemoveOutput(outputs[i]);
            }
        }
        protected static Creator CreateCreator()
        {
            return new Creator("BytingLib", null, null, typeof(InputShortcutAttribute))
            {
                ParameterSeparator = ','
            };
        }

        public string Serialize()
        {
            string output = "";

            Creator c = CreateCreator();

            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                try
                {
                    if (output != "")
                    {
                        output += ",";
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
                    output += prop.Name + ":" + c.Serialize(pointerVal);
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
            Creator c = CreateCreator();
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
    }
}
