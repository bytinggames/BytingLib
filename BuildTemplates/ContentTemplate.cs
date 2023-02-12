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
            private readonly string fullName;
            private readonly string name;
            private readonly string extension;
            private readonly string? assetType;
            private readonly string? customPrintDeclare;
            private readonly string? customPrintInit;

            public File(string _name)
            {
                fullName = _name;
                name = _name;
                extension = Path.GetExtension(name)[1..];
                this.name = _name.Remove(_name.Length - extension.Length - 1);

                if (extension == "ani")
                {
                    customPrintDeclare = $"public Animation {ToVariableName(name)}Ani {{ get; }}";
                    customPrintInit = $"{ToVariableName(name)}Ani = disposables.Use(collector.UseAnimation(\"{{0}}{name}\"));";
                }
                else if (extension == "txt")
                {
                    customPrintDeclare = $"public Ref<string> {ToVariableName(name)}Txt {{ get; }}";
                    customPrintInit = $"{ToVariableName(name)}Txt = disposables.Use(collector.Use<string>(\"{{0}}{fullName}\"));";
                }
                else if (extension == "bin")
                {
                    customPrintDeclare = $"public Ref<byte[]> {ToVariableName(name)}Bytes {{ get; }}";
                    customPrintInit = $"{ToVariableName(name)}Bytes = disposables.Use(collector.Use<byte[]>(\"{{0}}{fullName}\"));";
                }
                else
                    assetType = AssetTypes.Convert(extension)!;
            }

            public string? PrintDeclare(string contentDirectory)
            {
                if (customPrintDeclare != null)
                    return customPrintDeclare;
                else
                {
                    if (assetType != null)
                    {
                        string VarName = AssetTypes.Extensions[assetType].VarName;
                        return $"public Ref<{assetType}> {ToVariableName(name)}{VarName} {{ get; }}";
                    }
                    else
                        return null;
                }
            }

            public string? PrintInit(string contentDirectory)
            {
                if (customPrintInit != null)
                    return string.Format(customPrintInit, contentDirectory);
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
                string? process = customContent.GetCustomCode(contentDirectory + fullName);
                if (process == null)
                    return null;

                return $@"#begin {contentDirectory}{fullName}{process}

";
            }
        }

        public static (string output, string mgcbOutput, string locaCode) Create(string contentPath, string nameSpace, string[] referencedDlls)
        {
            if (!contentPath.EndsWith("/") && !contentPath.EndsWith("\\"))
                contentPath += "/";

            Folder root = new("Content", "ContentLoader");

            List<string> locaFiles = new();
            LookIntoDirRecursive(contentPath, contentPath, root, ref locaFiles);

            string output = root.Print("", Folder.tab);

            CustomContent customContent = new(contentPath);
            string mgcbOutput = root.PrintMGCB("", referencedDlls, customContent);

            string locaCode = LocaGenerator.Generate(nameSpace, locaFiles.ToArray());

            return (output, mgcbOutput, locaCode);
        }

        private static void LookIntoDirRecursive(string contentPath, string currentPath, Folder root, ref List<string> locaFiles)
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

                root.Insert(localAssetPath);
            }

            var dirs = Directory.GetDirectories(currentPath).OrderBy(f => f);
            foreach (var dir in dirs)
            {
                string dirName = Path.GetFileName(dir);
                if (dirName != "bin" && dirName != "obj")
                    LookIntoDirRecursive(contentPath, dir, root, ref locaFiles);
            }
        }
    }
}
