using System.Text.RegularExpressions;

namespace BytingLib
{
    public class Input : IDisposable
    {
        private InputUpdate[] outputs;
        private readonly InputUpdater updater;

        public Input(InputUpdater updater)
        {
            outputs = GetOutputs();

            for (int i = 0; i < outputs.Length; i++)
            {
                updater.AddOutput(outputs[i]);
                outputs[i].Initialize(updater);
            }

            this.updater = updater;
        }

        private InputUpdate[] GetOutputs()
        {
            List<InputUpdate> outputsList = new();

            var props = GetType().GetProperties();

            foreach (var prop in props)
            {
                if (!prop.PropertyType.IsAssignableTo(typeof(InputUpdate)))
                {
                    continue;
                }
                InputUpdate? instance = (InputUpdate?)prop.GetValue(this);
                if (instance == null)
                {
                    continue;
                }
                outputsList.Add(instance);
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
    }
}
