using System.Text.Json;

namespace BytingLib
{
    public class BindsSerializer
    {
        private Dictionary<Type, List<string>> defaultJsonSplit { get; } = new();
        private static readonly JsonSerializerOptions options = new()
        {
             
        };

        public string Serialize(object inputBinds, bool removeDefaults = true)
        {
            Type type = inputBinds.GetType();
            string json = JsonSerializer.Serialize(inputBinds, type, options);

            if (removeDefaults)
            {
                json = RemoveDefaultJson(json, GetDefaultJsonSplit(type));
            }
            return json;
        }

        private string RemoveDefaultJson(string json, List<string> defaultJson)
        {
            List<string> split = SplitJson(json);
            for (int i = 0; i < split.Count; i++)
            {
                int j;
                for (j = 0; j < defaultJson.Count; j++)
                {
                    if (split[i] == defaultJson[j])
                    {
                        break;
                    }
                }
                if (j < defaultJson.Count)
                {
                    split.RemoveAt(i--);
                }
            }

            return "{" + string.Join(",", split) + "}";
        }

        private List<string> SplitJson(string json)
        {
            ScriptReader reader = new(json);
            if (reader.ReadChar() != '{')
            {
                return new();
            }

            List<string> splits = new();
            do
            {
                // WARNING: this doesn't work if the number of { is unequal to } inside a string
                splits.Add(reader.ReadToCharOrEndConsiderOpenCloseBraces([',', '}'], '{', '}'));
            }
            while (!reader.EndOfString());

            return splits;
        }

        /// <summary>
        /// Setting replaceOrAdd to false is only used for debugging (only supports Bool and Vector2 and creates new references each time you call this method)
        /// </summary>
        public void Deserialize(string json, InputBinds binds, bool replaceOrAdd = true, bool onlyOverrideIfDefinedInJson = false)
        {
            HashSet<string>? propsDefinedInJson = null;

            if (onlyOverrideIfDefinedInJson)
            {
                propsDefinedInJson = new();
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    foreach (var element in doc.RootElement.EnumerateObject())
                    {
                        propsDefinedInJson.Add(element.Name);
                    }
                }
            }

            var newBinds = JsonSerializer.Deserialize(json, binds.GetType(), options);
            if (newBinds != null)
            {
                foreach (var prop in binds.GetRemappableProperties())
                {
                    if (onlyOverrideIfDefinedInJson && !propsDefinedInJson!.Contains(prop.Name))
                    {
                        continue;
                    }
                    var newVal = prop.GetValue(newBinds);
                    if (newVal is Input newInput)
                    {
                        var currentVal = prop.GetValue(binds);
                        if (currentVal == null)
                        {
                            prop.SetValue(binds, newInput);
                        }
                        else
                        {
                            if (currentVal is Input input)
                            {
                                if (replaceOrAdd)
                                {
                                    input.Override(newInput, f => prop.SetValue(binds, f));
                                }
                                else
                                {
                                    if (input is InputBool inputBool && newInput is InputBool newInputBool)
                                    {
                                        inputBool.Override(new BoolOr(inputBool, newInputBool), f => prop.SetValue(binds, f));
                                    }
                                    else if (input is InputVector2 inputV && newInput is InputVector2 newInputV)
                                    {
                                        inputV.Override(new Vector2MaxLength(inputV, newInputV), f => prop.SetValue(binds, f));
                                    }
                                    else
                                    {
                                        input.Override(newInput, f => prop.SetValue(binds, f));
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("currentVal is not of type Input");
                            }
                        }
                    }
                    else
                    {
                        prop.SetValue(binds, newVal);
                    }
                }
            }
        }

        private List<string> GetDefaultJsonSplit(Type t)
        {
            if (!defaultJsonSplit.ContainsKey(t))
            {
                string json = JsonSerializer.Serialize(Activator.CreateInstance(t), t, options);
                defaultJsonSplit.Add(t, SplitJson(json)); 
            }
            return defaultJsonSplit[t];
        }

        /// <summary>Can also be used on Inputs</summary>
        public bool AreEqualWhenSerialized(object? obj1, object? obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            if (obj1 == null || obj2 == null)
            {
                return false;
            }
            return Serialize(obj1, false) == Serialize(obj2, false);
        }
    }
}
