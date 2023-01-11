using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace BuildTemplates
{
    public class ContentTemplate
    {
        class Folder
        {
            private readonly string name;
            private readonly string className;

            public Dictionary<string, Folder> folders = new();

            public List<File> files = new List<File>();

            public Folder(string name, string? customClassName = null)
            {
                this.name = name;
                this.className = customClassName ?? ("_" + name);
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
                    folderConstruct += "\t\t" + $"{folder.Value.name} = new _{folder.Value.name}(collector, disposables);" + endl;
                    classes += "\t" + folder.Value.Print(contentDirectory + folder.Value.name, tabs + "\t") + endl;
                }

                foreach (var file in files)
                {
                    assets += "\t" + file.Print(contentDirectory) + endl;
                }

                string output = $@"
public class {className}
{{
{folderProperties}
    
    protected readonly IContentCollector collector;
    protected readonly DisposableContainer disposables;
    public {className}(IContentCollector collector, DisposableContainer disposables)
    {{
        this.collector = collector;
        this.disposables = disposables;
{folderConstruct}
    }}

{assets}
     
{classes}
}}
";
                return output.Replace("\n", "\n" + tabs);
            }

            public void GetFilesRecursively(List<File> allFiles)
            {
                allFiles.AddRange(files);
                foreach (var folder in folders)
                {
                    folder.Value.GetFilesRecursively(allFiles);
                }
            }

            private void PrintMGCBRecursively(string contentDirectory, ref string assets)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                foreach (var file in files)
                {
                    assets += file.PrintMGCB(contentDirectory);
                }

                foreach (var folder in folders)
                {
                    folder.Value.PrintMGCBRecursively(contentDirectory + folder.Value.name, ref assets);
                }
            }

            public string PrintMGCB(string contentDirectory)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                string endl = "\r\n";
                string references = "";
                string assets = "";


                //List<File> allFiles = new List<File>();
                //GetFilesRecursively(allFiles);

                PrintMGCBRecursively(contentDirectory, ref assets);

                string output = $@"
#----------------------------- Global Properties ----------------------------#
/outputDir:bin/$(Platform)
/intermediateDir:obj/$(Platform)
/platform:DesktopGL
/config:
/profile:Reach
/compress:False

#-------------------------------- References --------------------------------#
{references}

#---------------------------------- Content ---------------------------------#
{assets}
";
                return output;
            }
        }
        class File
        {
            private readonly string fullName;
            private readonly string name;
            private readonly string extension;

            private readonly string assetType;

            private string? customPrint;

            public File(string _name)
            {
                fullName = _name;
                name = _name;
                extension = Path.GetExtension(name)[1..];
                this.name = _name.Remove(_name.Length - extension.Length - 1);

                if (extension == "ani")
                {
                    customPrint = $"public Animation {ToVariableName(name)}Ani => disposables.Use(collector.UseAnimation(\"{{0}}{name}\"));";
                }
                else
                if (extension == "txt")
                {
                    customPrint = $"public Ref<string> {ToVariableName(name)}Txt => disposables.Use(collector.Use<string>(\"{{0}}{_name}\"));";
                }
                else
                    assetType = AssetTypes.Convert(extension)!;
            }

            public string Print(string contentDirectory)
            {
                if (customPrint != null)
                    return string.Format(customPrint, contentDirectory);
                else
                {
                    string VarName = AssetTypes.Extensions[assetType].VarName;
                    return $"public Ref<{assetType}> {ToVariableName(name)}{VarName} => disposables.Use(collector.Use<{assetType}>(\"{contentDirectory + name}\"));";
                }
            }

            static string ToVariableName(string name)
            {
                return name.Replace(" ", "")
                    .Replace(".", "_")
                    .Replace(";", "_")
                    .Replace("-", "_");
            }



            public string PrintMGCB(string contentDirectory)
            {
                string buildProcess;
                switch (extension)
                {
                    case "png":
                    case "jpg":
                    case "jpeg":
                        buildProcess = @"
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyColor=255,0,255,255
/processorParam:ColorKeyEnabled=True
/processorParam:GenerateMipmaps=False
/processorParam:PremultiplyAlpha=True
/processorParam:ResizeToPowerOfTwo=False
/processorParam:MakeSquare=False
/processorParam:TextureFormat=Color";
                        break;

                    case "wav":
                        buildProcess = @"
/importer:WavImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best";
                        break;

                    case "ogg":
                        buildProcess = @"
/importer:OggImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best";
                        break;

                    case "spritefont":
                        buildProcess = @"
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/processorParam:PremultiplyAlpha=True
/processorParam:TextureFormat=Compressed";
                        break;

                    case "fx":
                        buildProcess = @"
/importer:EffectImporter
/processor:EffectProcessor
/processorParam:DebugMode=Auto";
                        break;

                    case "fbx":
                        buildProcess = @"
/importer:FbxImporter
/processor:MyModelProcessor";
                        break;
                    default:
                        return "# couldn't generate mgcb code for file " + fullName;
                }



                return $@"#begin {contentDirectory}{fullName}{buildProcess}
/build:{contentDirectory}{fullName}
";

//                return $@"#begin {fullName}
///copy:{fullName}
//";
            }
        }

        public static (string output, string mgcbOutput) Create(string contentPath)
        {
            if (!contentPath.EndsWith("/"))
                contentPath += "/";

            Folder root = new Folder("Content", "ContentLoader");

            LookIntoDirRecursive(contentPath, contentPath, root);

            string output = root.Print("", "");

            output = Regex.Replace(output, "\n( |\t)*\r", "");

            string mgcbOutput = root.PrintMGCB("");

            return (output, mgcbOutput);
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
