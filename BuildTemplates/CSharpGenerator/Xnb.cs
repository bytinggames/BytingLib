namespace BuildTemplates
{
    public class Xnb
    {
        public string AssetName { get; }
        public string FilePath { get; }
        /// <summary>The same as asset name but without the parent folders</summary>
        public string FileName { get; set; }
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);
        public string CSharpDataType { get; }
        public string VarNameExtension { get; }

        string VarName => $"{ToVariableName(FileNameWithoutExtension)}{VarNameExtension}";

        public Xnb(string filePath, string cSharpDataType, string varNameExtension)
        {
            AssetName = FileName = FilePath = filePath;
            CSharpDataType = cSharpDataType;
            VarNameExtension = varNameExtension;

            int dotIndex = AssetName.LastIndexOf('.');
            if (dotIndex != -1)
                AssetName = AssetName.Remove(dotIndex);
        }

        public override string ToString()
        {
            return $"{FileName} {CSharpDataType} {VarNameExtension}";
        }

        public string? PrintDeclare(bool loadOnStartup)
        {
            if (loadOnStartup)
                return $"public Ref<{CSharpDataType}> {VarName} {{ get; }}";
            else
                return $"public Ref<{CSharpDataType}> {GetInitCode()}";
        }

        public string? PrintInit(bool loadOnStartup)
        {
            if (loadOnStartup)
            {
                return GetInitCode();
            }
            else
                return null;
        }

        private string GetInitCode() => $"{VarName} => disposables.Use(collector.Use<{CSharpDataType}>(\"{AssetName}\"));";

        static string ToVariableName(string name)
        {
            return name.Replace(" ", "")
                .Replace(".", "_")
                .Replace(";", "_")
                .Replace("-", "_");
        }

    }
}
