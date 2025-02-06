using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace BytingLib
{
    public class InputBinds
    {
        protected static InputBool Ctrl() => new BoolCtrl();
        protected static InputBool Shift() => new BoolShift();
        protected static InputBool Alt() => new BoolAlt();
        protected static InputBool And(params InputBool[] inputs) => new BoolAnd(inputs);
        protected static InputBool Or(params InputBool[] inputs) => new BoolOr(inputs);
        protected static InputBool Not(InputBool input) => new BoolNot(input);


        private readonly List<string> defaultJsonSplit;
        private static readonly JsonSerializerOptions options = new();

        public InputBinds()
        {
            string json = JsonSerializer.Serialize(this, options);
            defaultJsonSplit = SplitJson(json);
        }
        public string Serialize()
        {
            string json = JsonSerializer.Serialize(this, options);

            return RemoveDefaultJson(json);
        }

        private string RemoveDefaultJson(string json)
        {
            List<string> split = SplitJson(json);
            for (int i = 0; i < split.Count; i++)
            {
                int j;
                for (j = 0; j < defaultJsonSplit.Count; j++)
                {
                    if (split[i] == defaultJsonSplit[j])
                    {
                        break;
                    }
                }
                if (j < defaultJsonSplit.Count)
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

        public void Deserialize(string json)
        {
            var newBinds = JsonSerializer.Deserialize(json, GetType(), options);
            if (newBinds != null)
            {
                foreach (var prop in GetRemappableProperties())
                {
                    var newVal = prop.GetValue(newBinds);
                    if (newVal is Input newInput)
                    {
                        var currentVal = prop.GetValue(this);
                        if (currentVal is Input input)
                        {
                            input.Override(newInput, f => prop.SetValue(this, f));
                        }
                        else
                        {
                            throw new Exception("currentVal is not of type Input");
                        }
                    }
                    else
                    {
                        prop.SetValue(this, newVal);
                    }
                }
            }
        }

        public IEnumerable<PropertyInfo> GetRemappableProperties()
        {
            var props = GetType().GetProperties(
                BindingFlags.SetProperty
                | BindingFlags.GetProperty
                | BindingFlags.Public
                | BindingFlags.Instance
            ).Where(f => f.CanWrite && f.CanRead);

            return props;
        }
    }
}
