using BytingLib;
using System;

namespace BuildTemplates
{
    public class ShaderGenerator
    {
        internal static ShaderFile[] Generate(string nameSpace, List<string> fxFiles, string effectRootDir, string shaderOutputDir)
        {
            ShaderFile[] shaders = new ShaderFile[fxFiles.Count];

            for (int i = 0; i < shaders.Length; i++)
                shaders[i] = new ShaderFile(nameSpace, fxFiles[i], effectRootDir, shaderOutputDir);

            return shaders;
        }
    }

    public class ShaderFile
    {
        private readonly string nameSpace;
        private string className;

        string outputFile;
        string declareCode = "";
        string ctorParameter = "";
        string initCode = "";

        string outputCode;

        HashSet<string> includeFiles = new();

        public ShaderFile(string nameSpace, string file, string effectRootDir, string shaderOutputDir)
        {
            this.nameSpace = nameSpace;
            this.className = Path.GetFileNameWithoutExtension(file)!;
            string localPath = file.Substring(effectRootDir.Length + 1);
            localPath = localPath.Remove(localPath.Length - 3); // remove .fx
            string fileName = Path.GetFileName(localPath);
            string localDir = Path.GetDirectoryName(localPath)!;
            localPath = Path.Combine(localDir, "Shader" + fileName + ".Generated.cs");
            outputFile = Path.Combine(shaderOutputDir, localPath);

            ReadFile(file);

            outputCode = CreateFileCode();
        }

        private string CreateFileCode()
        {
            return @$"namespace {nameSpace}
{{
    public partial class Shader{className} : Shader
    {{
{declareCode}
        public Shader{className}(Ref<Effect> effect{ctorParameter})
            : base(effect)
        {{
{initCode}
			Initialize();
        }}
    }}
}}

";
        }

        public void WriteToFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            File.WriteAllText(outputFile, outputCode);
        }

        private void ReadFile(string file)
        {
            ScriptReader reader = new ScriptReader(File.ReadAllText(file));
            bool endReached;
            while (true)
            {
                // iterate over includes
                reader.ReadToStringOrEnd("#include \"", out endReached);
                if (endReached)
                    break;
                string relativeFileName = reader.ReadToCharOrEnd(out char? foundChar, '"');
                if (foundChar == null)
                    break;

                string includeFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, relativeFileName));
                
                if (includeFiles.Add(includeFile)) // prevent duplicates
                    ReadFile(includeFile);
            }
            // get variables
            reader.SetPosition(0);

            while (true)
            {
                reader.ReadToStringOrEnd("//+C#", out endReached);
                if (endReached)
                    break;

                string variableCode = reader.ReadToStringOrEnd("//-C#", out endReached);

                if (endReached)
                    throw new Exception("'//-C#' missing after '//+C#' starts a variable region");

                variableCode = variableCode.Replace("\r", "");
                string[] lineBlock = variableCode.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lineBlock)
                {
                    int firstSpaceIndex = line.IndexOf(" ");
                    if (firstSpaceIndex == -1)
                        continue;
                    int semicolonIndex = line.IndexOf(";", firstSpaceIndex);
                    if (semicolonIndex == -1)
                        continue;
                    int commentIndex = line.IndexOf("//");
                    if (commentIndex != -1 && commentIndex < semicolonIndex)
                        continue;
                    string hlslVarType = line.Remove(firstSpaceIndex);
                    string[] hlslVarNames = line.Substring(firstSpaceIndex + 1, semicolonIndex - (firstSpaceIndex + 1)).Split(new char[] { ',' });
                    bool[] array = new bool[hlslVarNames.Length];

                    for (int i = 0; i < hlslVarNames.Length; i++)
                    {
                        int arrStart = hlslVarNames[i].IndexOf("[");
                        if (arrStart == -1)
                            continue;
                        int arrEnd = hlslVarNames[i].IndexOf("]");
                        if (arrEnd == -1)
                            continue;
                        array[i] = true;
                        hlslVarNames[i] = hlslVarNames[i].Remove(arrStart, arrEnd + 1 - arrStart);
                    }

                    string cSharpDataType = hlslVarType switch
                    {
                        "float2" => "Vector2",
                        "float3" => "Vector3",
                        "float4" => "Vector4",
                        "float3x3" => "Matrix",
                        "float4x4" => "Matrix",
                        "matrix" => "Matrix",
                        "texture2D" => "Texture2D",
                        "texture" => "Texture2D",
                        "uint" => "int", // cause MonoGame doesn't support setting uint parameters (at least not that I know of)
                        _ => hlslVarType
                    };


                    string _override = "";
                    string _accessor = "public ";
                    bool textureSamplerExists = false;
                    if (commentIndex != -1)
                    {
                        commentIndex += 2;
                        string comment = line.Substring(commentIndex);
                        string[] commands = comment.Split(new char[] { '|' });
                        foreach (string c in commands)
                        {
                            if (c == "override")
                                _override = "override ";
                            else if (c == "private")
                                _accessor = "private ";
                            else if (c == "sampler-exists")
                                textureSamplerExists = true;
                        }
                    }

                    for (int i = 0; i < hlslVarNames.Length; i++)
                    {
                        string currentCSharpDataType = cSharpDataType;
                        if (array[i])
                            currentCSharpDataType += "[]";

                        hlslVarNames[i] = hlslVarNames[i].Trim();

                        declareCode += $"\t\t{_accessor}{_override}EffectParameterStack<{currentCSharpDataType}> {hlslVarNames[i]} {{ get; }}\n";

                        string parameterVariable = hlslVarNames[i];
                        parameterVariable = parameterVariable[0].ToString().ToLower() + parameterVariable.Substring(1);

                        ctorParameter += $", {currentCSharpDataType} {parameterVariable}";

                        string accessVar = hlslVarNames[i];
                        if (!textureSamplerExists && (hlslVarType == "texture2D" || hlslVarType == "Texture2D"))
                            accessVar = "Sampler+" + accessVar;

                        initCode += $"\t\t\tAddParam(this.{hlslVarNames[i]} = new(effect, \"{accessVar}\", {parameterVariable}));\n";
                    }
                }
            }
        }
    }
}
