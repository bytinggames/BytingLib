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
    
    private readonly IContentCollector collector;

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

            public File(string name)
            {
                this.name = name;

                string ext = Path.GetExtension(name);
                this.name = name.Remove(name.Length - ext.Length);
                ext = ext[1..];

                assetType = ext switch
                {
                    "png" or "jpeg" or "jpg" => "Texture2D",
                    _ => throw new NotImplementedException(),
                };
            }

            public string Print(string contentDirectory)
            {
                return $"public AssetRef<{assetType}> Use{ToVariableName(name)}() => collector.Use<{assetType}>(\"{contentDirectory + name}\");";
            }

            static string ToVariableName(string name)
            {
                return name.Replace(" ", "")
                    .Replace(".", "_")
                    .Replace(";", "_")
                    .Replace("-", "_");
            }
        }

        public static string Create()
        {
            //string path = Path.Combine(Environment.CurrentDirectory, "Content", "Textures");
            string path = @"D:\Documents\Visual Studio 2017\Projects\LevelSketch\LevelSketch\Content\";

            Folder root = new Folder("Content");

            foreach (var file in Directory.EnumerateFiles(path, "*.png" /* TODO */, SearchOption.AllDirectories))
            {
                // D:\Documents\Visual Studio 2017\Projects\LevelSketch\LevelSketch\Content\Textures\Enemy.png
                string localPath = file[path.Length..]; //Textures\Enemy.png
                //localPath = localPath.Remove(localPath.Length - Path.GetExtension(localPath).Length); //Textures\Enemy
                localPath = localPath.Replace('\\', '/'); //Textures/Enemy.png

                root.Insert(localPath);
            }

            string output = root.Print("", "");

            output = Regex.Replace(output, "\n( |\t)*\r", ""); 

            return output;
        }
    }
}
