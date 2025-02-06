using System.Text.Json;

namespace BytingLib
{
    public class BindsSerializer
    {
        private Dictionary<Type, List<string>> defaultJsonSplit { get; } = new();
        private static readonly JsonSerializerOptions options = new()
        {
             
        };

        public string Serialize(InputBinds binds)
        {
            Type type = binds.GetType();
            string json = JsonSerializer.Serialize(binds, type, options);

            return RemoveDefaultJson(json, GetDefaultJsonSplit(type));
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

        public void Deserialize(string json, InputBinds binds)
        {
            var newBinds = JsonSerializer.Deserialize(json, binds.GetType(), options);
            if (newBinds != null)
            {
                foreach (var prop in binds.GetRemappableProperties())
                {
                    var newVal = prop.GetValue(newBinds);
                    if (newVal is Input newInput)
                    {
                        var currentVal = prop.GetValue(binds);
                        if (currentVal is Input input)
                        {
                            input.Override(newInput, f => prop.SetValue(binds, f));
                        }
                        else
                        {
                            throw new Exception("currentVal is not of type Input");
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
    }
}
