namespace BuildTemplates
{
    public class Xnb
    {
        public string AssetName { get; }
        public string FilePath { get; }
        /// <summary>The same as asset name but without the extension</summary>
        public string FileName { get; set; }
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);
        public string CSharpDataType { get; }
        public string VarNameExtension { get; }

        string VarName => $"{ToVariableName(FileNameWithoutExtension)}{VarNameExtension}";

        public Xnb(string filePath, string cSharpDataType, string varNameExtension)
        {
            AssetName = Path.GetFileNameWithoutExtension(filePath);

            FileName = FilePath = filePath;
            CSharpDataType = cSharpDataType;
            VarNameExtension = varNameExtension;
        }

        public override string ToString()
        {
            return $"{FileName} {CSharpDataType} {VarNameExtension}";
        }

        public string Print(bool loadOnStartup, string tab)
        {
            if (loadOnStartup)
            {
                return $"public Ref<{CSharpDataType}> {VarName} {{ get; }} = d.Use<{CSharpDataType}>(Path + \"{AssetName}\");";
            }
            else
            {
                return $"public Ref<{CSharpDataType}> {VarName} => _{VarName}.Use();";
            }
        }

        public string PrintRefLoader(bool loadOnStartup, string tab)
        {
            if (loadOnStartup)
            {
                return $"";
            }
            else
            {
                return $"public RefLoader<{CSharpDataType}> _{VarName} = new(d, Path +  \"{AssetName}\");";
            }
        }

        internal static string ToVariableName(string name)
        {
            if (name.Length == 0)
            {
                throw new Exception("name must have a length of > 0");
            }

            if (name[0] >= '0' && name[0] <= '9')
            {
                name = "_" + name;
            }

            return name.Replace(" ", "")
                .Replace(".", "_")
                .Replace(";", "_")
                .Replace("-", "_");
        }

    }
}
