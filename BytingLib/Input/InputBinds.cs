using System.Reflection;

namespace BytingLib
{
    public class InputBinds<T> where T : class
    {
        public T Binds { get; private set; } = Activator.CreateInstance<T>();
        private readonly List<string> defaultJsonSplit;

        public InputBinds()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(Binds, options);
            defaultJsonSplit = SplitJson(json);
        }

        System.Text.Json.JsonSerializerOptions options = new();

        public string Serialize()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(Binds, options);

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
            var newBinds = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
            if (newBinds != null)
            {
                // TODO: automate
                //Binds.Rewind.Override(newBinds.Rewind, f => Binds.Rewind = f);
                //Binds.Confirm.Override(newBinds.Confirm, f => Binds.Confirm = f);
                //Binds.Cancel.Override(newBinds.Cancel, f => Binds.Cancel = f);
                //Binds.Crouch.Override(newBinds.Crouch, f => Binds.Crouch = f);
                throw new Exception("TODO");
            }
        }

        public PropertyInfo[] GetRemappableProperties()
        {
            var props = Binds.GetType().GetProperties();

            return props;
            //foreach (var prop in props)
            //{
            //    IPointerValue? instance = null;
            //    try
            //    {
            //        var ignoreAttr = prop.GetCustomAttribute<CreatorIgnoreAttribute>(true);
            //        if (ignoreAttr != null)
            //        {
            //            continue;
            //        }
            //        if (!prop.PropertyType.IsAssignableTo(typeof(IPointerValue)))
            //        {
            //            continue;
            //        }
            //        instance = (IPointerValue?)prop.GetValue(this);
            //        if (instance == null)
            //        {
            //            continue;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        updater.OnException.Invoke(e);
            //    }
            //    if (instance != null)
            //    {
            //        yield return new(prop.Name, instance);
            //    }
            //}
        }
    }
}