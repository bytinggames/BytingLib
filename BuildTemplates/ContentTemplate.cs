using BytingLib;

namespace BuildTemplates
{
    public class ContentTemplate
    {
        class Folder
        {
            private readonly string name;

            public Dictionary<string, Folder> folders = new();

            public List<File> files = new();

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
/keepDuplicates:True

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

            public File(string _name)
            {
                FullName = _name;
                Extension = Path.GetExtension(_name)[1..];
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

        public static (string cSharpOutput, string mgcbOutput, string locaCode, ShaderFile[] shaders) 
            Create(string contentPath, string nameSpace, string[] referencedDlls, bool loadOnStartup,
            ContentConverter contentConverter)
        {
            if (!contentPath.EndsWith("/") && !contentPath.EndsWith("\\"))
                contentPath += "/";

            Folder root = new("Content");

            List<string> locaFiles = new();
            LookIntoDirRecursive(contentPath, contentPath, root, ref locaFiles);

            List<string> fxFiles = new();
            root.GetFileNamesByExtensionRecursively(fxFiles, "fx", Path.GetFullPath(Path.Combine(contentPath, "..")));
            const string effectRootRelative = "Effects";
            for (int i = fxFiles.Count - 1; i >= 0; i--)
            {
                if (!fxFiles[i].StartsWith(Path.GetFullPath(Path.Combine(contentPath, effectRootRelative + Path.DirectorySeparatorChar))))
                    fxFiles.RemoveAt(i);
            }

            CustomContent customContent = new(contentPath);
            string mgcbOutput = root.PrintMGCB("", referencedDlls, customContent);

            XnbFolder mgcbOutputFolder = CSharpGenerator.FromMGCB(mgcbOutput, contentConverter);
            string cSharpOutput = mgcbOutputFolder.Print("", XnbFolder.tab, loadOnStartup);

            string locaCode = LocaGenerator.Generate(nameSpace, locaFiles.ToArray());

            string effectRootDir = Path.Combine(contentPath, effectRootRelative);
            ShaderFile[] shaders = ShaderGenerator.Generate(nameSpace, fxFiles, effectRootDir, Path.GetFullPath(Path.Combine(contentPath, "..", "Shaders")));

            return (cSharpOutput, mgcbOutput, locaCode, shaders);
        }

        private static void LookIntoDirRecursive(string contentPath, string currentPath, Folder root, ref List<string> locaFiles)
        {
            var files = Directory.GetFiles(currentPath).OrderBy(f => f);
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file);
                ext = ext[1..];
                if (ext == "loca")
                {
                    locaFiles.Add(file);
                    continue; // TODO: what is this locaFiles about? shouldn't I remove this?
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
