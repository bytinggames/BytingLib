using System.Linq;

namespace BuildTemplates
{
    public class ContentTemplate
    {
        class Folder
        {
            private readonly string name;
            private readonly string className;

            public Dictionary<string, Folder> folders = new();

            public List<File> files = new();

            public Folder(string name, string? customClassName = null)
            {
                this.name = name;
                this.className = customClassName ?? ("_" + name);
            }

            internal void Insert(string localPath, bool loadOnStartup)
            {
                int slashIndex = localPath.IndexOf('/');
                if (slashIndex == -1)
                {
                    files.Add(new File(localPath, loadOnStartup));
                    return;
                }
                string nextDirectory = localPath.Remove(slashIndex);
                localPath = localPath[(slashIndex + 1)..];

                if (!folders.TryGetValue(nextDirectory, out Folder? folder))
                    folders.Add(nextDirectory, folder = new Folder(nextDirectory));
                folder.Insert(localPath, loadOnStartup);
            }

            const string endl = "\r\n";
            public const string tab = "    ";
        
            public string Print(string contentDirectory, string tabs)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                string folderProperties = "";
                string folderConstruct = "";
                string assets = "";
                string classes = "";
                string fieldInitialize = "";


                foreach (var folder in folders)
                {
                    folderProperties += endl + tab + $"public _{folder.Value.name} {folder.Value.name} {{ get; }}";
                    folderConstruct += endl + tab + tab + $"{folder.Value.name} = new _{folder.Value.name}(collector, disposables);";
                    classes += endl + tab + folder.Value.Print(contentDirectory + folder.Value.name, tabs);
                }

                foreach (var file in files)
                {
                    string? print = file.PrintDeclare(contentDirectory);
                    if (print == null)
                        continue;
                    assets += endl + tab + print;

                    print = file.PrintInit(contentDirectory);
                    if (!string.IsNullOrEmpty(print))
                        fieldInitialize += endl + tab + tab + print;
                }

                string output = $@"public class {className}
{{{folderProperties}
{tab}protected readonly IContentCollector collector;
{tab}protected readonly DisposableContainer disposables;
{tab}public {className}(IContentCollector collector, DisposableContainer disposables)
{tab}{{
{tab}{tab}this.collector = collector;
{tab}{tab}this.disposables = disposables;{fieldInitialize}{folderConstruct}
{tab}}}
{tab}public Ref<T> Use<T>(string assetName)
{tab}{{
{tab}{tab}return disposables.Use(collector.Use<T>(assetName));
{tab}}}{assets}{classes}
}}";
                return output.Replace("\r\n", "\n") // make consistent among OSs
                    .Replace("\n", "\n" + tabs); // indent
            }

            public void GetFilesRecursively(List<File> allFiles)
            {
                allFiles.AddRange(files);
                foreach (var folder in folders)
                {
                    folder.Value.GetFilesRecursively(allFiles);
                }
            }

            internal void GetFileNamesByExtensionRecursively(List<string> collectedFiles, string extension, string currentDir)
            {
                currentDir = Path.Combine(currentDir, name);
                for (int i = 0; i < files.Count; i++)
                {
                    if (files[i].Extension == extension)
                        collectedFiles.Add(Path.Combine(currentDir, files[i].FullName));
                }
                foreach (var folder in folders)
                {
                    folder.Value.GetFileNamesByExtensionRecursively(collectedFiles, extension, currentDir);
                }
            }

            private void PrintMGCBRecursively(string contentDirectory, ref string assets, CustomContent customContent)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                foreach (var file in files)
                {
                    assets += file.PrintMGCB(contentDirectory, customContent);
                }

                foreach (var folder in folders)
                {
                    folder.Value.PrintMGCBRecursively(contentDirectory + folder.Value.name, ref assets, customContent);
                }
            }

            public string PrintMGCB(string contentDirectory, string[] referencedDlls, CustomContent customContent)
            {
                if (!string.IsNullOrEmpty(contentDirectory))
                    contentDirectory += "/";

                string references = string.Join("\n", referencedDlls.Select(f => "/reference:" + f));
                string assets = "";


                //List<File> allFiles = new List<File>();
                //GetFilesRecursively(allFiles);

                PrintMGCBRecursively(contentDirectory, ref assets, customContent);

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
            public string FullName { get; }
            public string Extension { get; }
            private readonly bool loadOnStartup;
            private readonly string name;
            private readonly string? assetType;
            private readonly string? customPrintDeclare;
            private readonly string? customPrintInit;

            public File(string _name, bool loadOnStartup)
            {
                FullName = _name;
                this.loadOnStartup = loadOnStartup;
                name = _name;
                Extension = Path.GetExtension(name)[1..];
                this.name = _name.Remove(_name.Length - Extension.Length - 1);

                string? declare = null;
                string? init = null;
                string? varName = null;

                if (Extension == "ani")
                {
                    varName = ToVariableName(name) + "Ani";
                    declare = $"public Animation";
                    init = $"disposables.Use(collector.UseAnimation(\"{{0}}{name}\"))";
                }
                else if (Extension == "txt")
                {
                    varName = ToVariableName(name) + "Txt";
                    declare = $"public Ref<string>";
                    init = $"disposables.Use(collector.Use<string>(\"{{0}}{FullName}\"))";
                }
                else if (Extension == "bin")
                {
                    varName = ToVariableName(name) + "Bytes";
                    declare = $"public Ref<byte[]>";
                    init = $"disposables.Use(collector.Use<byte[]>(\"{{0}}{FullName}\"))";
                }
                else
                    assetType = AssetTypes.Convert(Extension)!;

                if (declare != null)
                {
                    if (loadOnStartup)
                    {
                        customPrintDeclare = $"{declare} {varName} {{ get; }}";
                        customPrintInit = $"{varName} = {init};";
                    }
                    else
                    {
                        customPrintDeclare = $"{declare} {varName} => {init};";
                        customPrintInit = null;
                    }
                }
            }

            public string? PrintDeclare(string contentDirectory)
            {
                if (customPrintDeclare != null)
                {
                    if (customPrintDeclare.Contains("{0}"))
                        return string.Format(customPrintDeclare, contentDirectory);
                    else
                        return customPrintDeclare;
                }
                else
                {
                    if (assetType != null)
                    {
                        string VarName = AssetTypes.Extensions[assetType].VarName;
                        if (loadOnStartup)
                            return $"public Ref<{assetType}> {ToVariableName(name)}{VarName} {{ get; }}";
                        else
                            return $"public Ref<{assetType}> {ToVariableName(name)}{VarName} => disposables.Use(collector.Use<{assetType}>(\"{contentDirectory + name}\"));";
                    }
                    else
                        return null;
                }
            }

            public string? PrintInit(string contentDirectory)
            {
                if (customPrintInit != null)
                    return string.Format(customPrintInit, contentDirectory);
                else if (!loadOnStartup)
                    return null;
                else
                {
                    if (assetType != null)
                    {
                        string VarName = AssetTypes.Extensions[assetType].VarName;
                        return $"{ToVariableName(name)}{VarName} = disposables.Use(collector.Use<{assetType}>(\"{contentDirectory + name}\"));";
                    }
                    else
                        return null;
                }
            }

            static string ToVariableName(string name)
            {
                return name.Replace(" ", "")
                    .Replace(".", "_")
                    .Replace(";", "_")
                    .Replace("-", "_");
            }



            public string? PrintMGCB(string contentDirectory, CustomContent customContent)
            {
                string? process = customContent.GetCustomCode(contentDirectory + FullName);
                if (process == null)
                    return null;

                return $@"#begin {contentDirectory}{FullName}{process}

";
            }
        }

        public static (string output, string mgcbOutput, string locaCode, ShaderFile[] shaders) Create(string contentPath, string nameSpace, string[] referencedDlls, bool loadOnStartup)
        {
            if (!contentPath.EndsWith("/") && !contentPath.EndsWith("\\"))
                contentPath += "/";

            Folder root = new("Content", "ContentLoader");

            List<string> locaFiles = new();
            LookIntoDirRecursive(contentPath, contentPath, root, ref locaFiles, loadOnStartup);

            List<string> fxFiles = new();
            root.GetFileNamesByExtensionRecursively(fxFiles, "fx", Path.GetFullPath(Path.Combine(contentPath, "..")));
            const string effectRootRelative = "Effects";
            for (int i = fxFiles.Count - 1; i >= 0; i--)
            {
                if (!fxFiles[i].StartsWith(Path.GetFullPath(Path.Combine(contentPath, effectRootRelative + Path.DirectorySeparatorChar))))
                    fxFiles.RemoveAt(i);
            }

            string output = root.Print("", Folder.tab);

            CustomContent customContent = new(contentPath);
            string mgcbOutput = root.PrintMGCB("", referencedDlls, customContent);

            string locaCode = LocaGenerator.Generate(nameSpace, locaFiles.ToArray());

            string effectRootDir = Path.Combine(contentPath, effectRootRelative);
            ShaderFile[] shaders = ShaderGenerator.Generate(nameSpace, fxFiles, effectRootDir, Path.GetFullPath(Path.Combine(contentPath, "..", "Rendering", "Shaders")));

            return (output, mgcbOutput, locaCode, shaders);
        }

        private static void LookIntoDirRecursive(string contentPath, string currentPath, Folder root, ref List<string> locaFiles, bool loadOnStartup)
        {
            var files = Directory.GetFiles(currentPath).OrderBy(f => f);
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file);
                ext = ext[1..];
                if (!AssetTypes.Extensions.Values.Any(f => f.Extensions.Any(g => g == ext)))
                {
                    if (ext == "loca")
                        locaFiles.Add(file);
                    continue;
                }

                string localAssetPath = file[contentPath.Length..];
                localAssetPath = localAssetPath.Replace('\\', '/');

                root.Insert(localAssetPath, loadOnStartup);
            }

            var dirs = Directory.GetDirectories(currentPath).OrderBy(f => f);
            foreach (var dir in dirs)
            {
                string dirName = Path.GetFileName(dir);
                if (dirName != "bin" && dirName != "obj")
                    LookIntoDirRecursive(contentPath, dir, root, ref locaFiles, loadOnStartup);
            }
        }
    }
}
