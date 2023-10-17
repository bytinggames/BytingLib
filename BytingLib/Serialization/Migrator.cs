using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BytingLib.Serialization
{
    public class Migrator<CurrentVersion>
    {
        private readonly JsonSerializerOptions jsonOptions;
        private readonly Dictionary<uint, Func<object, object?>> migrations;
        private readonly Type[] typePerVersion;

        /// <param name="typePerVersion">Excluding the CurrentVersion</param>
        public Migrator(JsonSerializerOptions jsonOptions, Dictionary<uint, Func<object, object?>> migrations, params Type[] typePerVersion)
        {
            this.jsonOptions = jsonOptions;
            this.migrations = migrations;

            if (migrations.Count != typePerVersion.Length)
            {
                throw new ArgumentException("migrations must be of same length as deprecatedTypes. Each deprecated type should be transformed to the next type.");
            }

            this.typePerVersion = typePerVersion.Concat(new Type[] { typeof(CurrentVersion) }).ToArray();
        }

        public uint StripVersionFromJson(ref string json)
        {
            if (string.IsNullOrEmpty(json)
                || json[0] != 'v')
            {
                return 0;
            }

            int braceIndex = json.IndexOf('{');
            if (braceIndex == -1)
            {
                return 0;
            }
            string versionStr = json.Substring(1, braceIndex - 1);
            versionStr = Regex.Replace(versionStr, @"\s+", string.Empty);
            if (!uint.TryParse(versionStr, out uint version))
            {
                return 0;
            }

            json = json.Substring(braceIndex);

            return version;
        }

        public void StripVersionFromJson(ref ReadOnlySpan<byte> json)
        {
            json = json.Slice(json.IndexOf(Encoding.UTF8.GetBytes("{"))); // skip migration version
        }

        public CurrentVersion? Deserialize(string json)
        {
            uint version = StripVersionFromJson(ref json);

            if (version >= typePerVersion.Length)
            {
                throw new ArgumentException("json migration version is too large");
            }

            Type type = typePerVersion[version];
            object? obj = JsonSerializer.Deserialize(json, type, jsonOptions);
            if (obj == null)
            {
                return default;
            }
            while (version < migrations.Count)
            {
                obj = migrations[version](obj);

                if (obj == null)
                {
                    return default;
                }

                version++;
            }

            return (CurrentVersion?)obj;
        }

        public string Serialize(CurrentVersion obj)
        {
            string json = JsonSerializer.Serialize(obj, jsonOptions);
            return "v" + migrations.Count + json;
        }
    }
}
