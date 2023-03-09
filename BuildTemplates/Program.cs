namespace BuildTemplates
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string nameSpace = args.Length == 0 ? "NAMESPACE" : args[0];
            string[] referencedDlls = new string[0];
            bool loadOnStartup = false;


            if (args.Length > 1)
            {
                referencedDlls = args[1].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (referencedDlls.Length == 1 && referencedDlls[0] == "_")
                    referencedDlls = new string[0];

                if (args.Length > 2)
                {
                    loadOnStartup = args[2].ToLower() == "true" || args[2] == "1";
                }
            }

            var projectPath = Environment.CurrentDirectory;
            string contentPath = Path.Combine(projectPath, "Content");
            (string code, string mgcbOutput, string locaCode, ShaderFile[] shaders) = ContentTemplate.Create(contentPath + "/", nameSpace, referencedDlls, loadOnStartup);

            string mgcbFile = Path.Combine(contentPath, "Content.Generated.mgcb");
            File.WriteAllText(mgcbFile, mgcbOutput);


            code = $@"// THIS IS A GENERATED FILE, DO NOT EDIT!
// generate it by saving the file '../_ContentGenerate.tt'. It should be located next to the *.csproj file.

using BytingLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace {nameSpace}
{{
    {code}
}}
";
            string contentFile = Path.Combine(contentPath, "ContentLoader.Generated.cs");
            File.WriteAllText(contentFile, code);

            string locaCodeFile = Path.Combine(contentPath, "Loca.Generated.cs");
            File.WriteAllText(locaCodeFile, locaCode);


            foreach (var shader in shaders)
            {
                shader.WriteToFile();
            }
        }
    }
}
