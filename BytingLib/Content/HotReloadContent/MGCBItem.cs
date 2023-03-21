namespace BytingLib
{
    public class MGCBItem
    {
        public string FilePath { get; }
        public string? Processor { get; }
        public string? CSharpType;
        public string? CSharpVarName;

        public MGCBItem(string filePath, string? processor, string? cSharpType, string? cSharpVarName)
        {
            FilePath = filePath;
            Processor = processor;
            CSharpType = cSharpType;
            CSharpVarName = cSharpVarName;
        }
    }
}
