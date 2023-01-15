using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildTemplates
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string nameSpace = args.Length == 0 ? "NAMESPACE" : args[0];

            var projectPath = Environment.CurrentDirectory;
            string contentPath = Path.Combine(projectPath, "Content");
            (string code, string mgcbOutput, string locaCode) = ContentTemplate.Create(contentPath + "/", nameSpace);

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
        }
    }
}
