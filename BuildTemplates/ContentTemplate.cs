using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BuildTemplates
{
    public class ContentTemplate
    {
        class Folder
        {
            private readonly string name;

            public Dictionary<string, Folder> folders = new();

            public List<File> files = new List<File>();

            public Folder(string name)
            {
                this.name = name;
            }

            internal void Insert(string localPath)
            {
                int slashIndex = localPath.IndexOf('/');
                if (slashIndex == -1)
                {
                    files.Add(new File(localPath));
                    return;
                }
                string nextDirectory = localPath.Remove(slashIndex);
                localPath = localPath[(slashIndex + 1)..];

                if (!folders.TryGetValue(nextDirectory, out Folder? folder))
                    folders.Add(nextDirectory, folder = new Folder(nextDirectory));
                folder.Insert(localPath);
            }

            public string Print(string contentDirectory, string tabs)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                string folderProperties = "";
                string folderConstruct = "";
                string assets = "";
                string classes = "";

                string endl = "\r\n";

                foreach (var folder in folders)
                {
                    folderProperties += "\t" + $"public _{folder.Value.name} {folder.Value.name} {{ get; }}" + endl;
                    folderConstruct += "\t\t" + $"{folder.Value.name} = new _{folder.Value.name}(collector);" + endl;
                    classes += "\t" + folder.Value.Print(contentDirectory + folder.Value.name, tabs + "\t") + endl;
                }

                foreach (var file in files)
                {
                    assets += "\t" + file.Print(contentDirectory) + endl;
                }

                string output = $@"
public class _{name}
{{
{folderProperties}
    
    protected readonly IContentCollector collector;

    public _{name}(IContentCollector collector)
    {{
        this.collector = collector;
{folderConstruct}
    }}

{assets}
     
{classes}
}}
";
                return output.Replace("\n", "\n" + tabs);
            }
        }
        class File
        {
            private readonly string name;

            private readonly string assetType;

            private string? customPrint;

            public File(string _name)
            {
                name = _name;
                string ext = Path.GetExtension(name);
                this.name = _name.Remove(_name.Length - ext.Length);

                if (ext == ".ani")
                {
                    customPrint = $"public Animation {ToVariableName(name)}Ani() => collector.UseAnimation(\"{{0}}{name}\");";
                }
                else
                if (ext == ".txt")
                {
                    customPrint = $"public Ref<string> {ToVariableName(name)}Txt() => collector.Use<string>(\"{{0}}{_name}\");";
                }
                else
                    assetType = AssetTypes.Convert(ext)!;
            }

            public string Print(string contentDirectory)
            {
                if (customPrint != null)
                    return string.Format(customPrint, contentDirectory);
                else
                {
                    string VarName = AssetTypes.Extensions[assetType].VarName;
                    return $"public Ref<{assetType}> {ToVariableName(name)}{VarName}() => collector.Use<{assetType}>(\"{contentDirectory + name}\");";
                }
            }

            static string ToVariableName(string name)
            {
                return name.Replace(" ", "")
                    .Replace(".", "_")
                    .Replace(";", "_")
                    .Replace("-", "_");
            }
        }

        public static string Create(string contentPath)
        {
            if (!contentPath.EndsWith("/"))
                contentPath += "/";

            Folder root = new Folder("Content");

            LookIntoDirRecursive(contentPath, contentPath, root);

            string output = root.Print("", "");

            output = Regex.Replace(output, "\n( |\t)*\r", ""); 

            return output;
        }

        private static void LookIntoDirRecursive(string contentPath, string currentPath, Folder root)
        {
            foreach (var file in Directory.EnumerateFiles(currentPath))
            {
                string ext = Path.GetExtension(file);
                ext = ext[1..];
                if (!AssetTypes.Extensions.Values.Any(f => f.Extensions.Any(g => g == ext)))
                    continue;

                string localAssetPath = file[contentPath.Length..];
                localAssetPath = localAssetPath.Replace('\\', '/');

                root.Insert(localAssetPath);
            }

            foreach (var dir in Directory.EnumerateDirectories(currentPath))
            {
                string dirName = Path.GetFileName(dir);
                if (dirName != "bin" && dirName != "obj")
                    LookIntoDirRecursive(contentPath, dir, root);
            }
        }
    }
}
