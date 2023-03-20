using System.Reflection;

namespace BytingLib
{
    public class HotReloadContent
    {
        public ContentManagerRaw TempContentRaw { get; }
        ContentBuilder contentBuilder;

        DirectorySupervisor dirSupervisor;
        IContentCollector content;
        GraphicsDevice gDevice;

        string sourceContentDir;

        /// <summary>Either localization.csv or any font changed.</summary>
        public event Action<string>? OnTextReload;

        readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        public HotReloadContent(IServiceProvider serviceProvider, IContentCollector content, string hotReloadContentPath)
        {
            this.content = content;

            sourceContentDir = Path.GetFullPath(hotReloadContentPath);

            bool expectEmptyDir;

            if (Path.GetFileName(sourceContentDir) == "Content"
                && Directory.EnumerateFiles(sourceContentDir, "*.mgcb", SearchOption.TopDirectoryOnly).Any()) // check if any mgcb file is present
                expectEmptyDir = false;
            else
                expectEmptyDir = true;

            //sourceContentDir = Paths.ModContent;// Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../../Content"));
            //bool expectEmptyDir = true;

            dirSupervisor = new DirectorySupervisor(sourceContentDir, GetFiles, expectEmptyDir);

            string tempPath = Path.Combine(sourceContentDir, "obj", "HotReload", "Content");
            string tempOutputPath = Path.Combine(sourceContentDir, "bin", "HotReload", "Content");

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            if (Directory.Exists(tempOutputPath))
                Directory.Delete(tempOutputPath, true);

            contentBuilder = new ContentBuilder(sourceContentDir, tempOutputPath, tempPath);

            TempContentRaw = new ContentManagerRaw(serviceProvider, contentBuilder.OutputPath);

            gDevice = (serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)!.GraphicsDevice;

            if (expectEmptyDir)
                UpdateChanges();
        }

        private string[] GetFiles()
        {
            List<string> files = new List<string>();
            //GetFromFolder("Effects", "*.fx|*.fxh");
            //GetFromFolder("Fonts", "*.xnb|*.spritefont");
            //GetFromFolder("Models", "*.fbx");
            //GetFromFolder("Music", "*.ogg");
            //GetFromFolder("Sounds", "*.ogg|*.wav");
            //GetFromFolder("Textures", "*.png|*.jpg|*.jpeg|*.ani");
            //GetFromFolder("", "*.txt|*.csv|*.json|*.xml|*.ini|*.config");
            Get("*.fx|*.fxh|*.xnb|*.spritefont|*.fbx|*.ogg|*.wav|*.png|*.jpg|*.jpeg|*.ani|*.txt|*.csv|*.json|*.xml|*.ini|*.config|*.gltf|*.bin|*.colmesh|*.colgrid");
            GetFile("Loca.loca");

            dependencies.Clear();
            InitEffectDependencies(files);
            InitGLTFDependencies(files);
            InitGLTFCollisionDependencies(files);

            //void GetFromFolder(string folder, string searchPattern)
            //{
            //    if (!Directory.Exists(Path.Combine(sourceContentDir, folder)))
            //        return;
            //    string[] searchPatterns = searchPattern.Split('|');
            //    foreach (string sp in searchPatterns)
            //    {
            //        files.AddRange(Directory.GetFiles(Path.Combine(sourceContentDir, folder), sp, SearchOption.AllDirectories));
            //    }
            //}

            void Get(string searchPattern)
            {
                if (!Directory.Exists(sourceContentDir))
                    return;
                string[] searchPatterns = searchPattern.Split('|');
                foreach (string sp in searchPatterns)
                {
                    files.AddRange(Directory.GetFiles(sourceContentDir, sp, SearchOption.TopDirectoryOnly));

                    foreach (string dir in Directory.GetDirectories(sourceContentDir))
                    {
                        string? dirName = Path.GetFileName(dir);
                        if (dirName == null || dirName == "bin" || dirName == "obj")
                            continue;
                        files.AddRange(Directory.GetFiles(dir, sp, SearchOption.AllDirectories));
                    }
                }
            }
            void GetFile(string file)
            {
                file = Path.Combine(sourceContentDir, file);
                if (File.Exists(file))
                    files.Add(file);
            }

            return files.ToArray();
        }

        private void InitEffectDependencies(IEnumerable<string> allFiles)
        {
            // add effect dependencies (fx depend on fxh's)
            const string includeStr = "#include \"";
            foreach (var f in allFiles.Where(f => f.EndsWith(".fx") || f.EndsWith(".fxh")))
            {
                string localFilePath = f.Substring(sourceContentDir.Length + 1);

                string shaderCode = File.ReadAllText(f);
                int i = 0;
                while ((i = shaderCode.IndexOf(includeStr, i)) != -1)
                {
                    i += includeStr.Length;
                    int i2 = shaderCode.IndexOf("\"", i);
                    string file = shaderCode.Substring(i, i2 - i);
                    file = Path.Combine(Path.GetDirectoryName(localFilePath)!, file);
                    if (!dependencies.ContainsKey(file))
                        dependencies.Add(file, new List<string>());
                    dependencies[file].Add(localFilePath);

                    i = i2 + 1;
                }
            }
        }
        private void InitGLTFDependencies(IEnumerable<string> allFiles)
        {
            // add effect dependencies (gltf depend on bin's)
            const string findBin = "\"uri\" : \"";
            foreach (var f in allFiles.Where(f => f.EndsWith(".gltf")))
            {
                string localFilePath = f.Substring(sourceContentDir.Length + 1);

                string json = File.ReadAllText(f);
                int i = 0;
                while ((i = json.IndexOf(findBin, i)) != -1)
                {
                    i += findBin.Length;
                    int i2 = json.IndexOf("\"", i);
                    string file = json.Substring(i, i2 - i);
                    file = Path.Combine(Path.GetDirectoryName(localFilePath)!, file);
                    if (!dependencies.ContainsKey(file))
                        dependencies.Add(file, new List<string>());
                    dependencies[file].Add(localFilePath);

                    i = i2 + 1;
                }
            }
        }
        private void InitGLTFCollisionDependencies(IEnumerable<string> allFiles)
        {
            // colgrid and colmesh depends on gltf
            foreach (var f in allFiles.Where(f => f.EndsWith(".colgrid") || f.EndsWith(".colmesh")))
            {
                string localFilePath = f.Substring(sourceContentDir.Length + 1);

                string gltfFile = localFilePath;
                const string colGridStr = "Col.colgrid";
                const string colMeshStr = "Col.colmesh";
                if (gltfFile.EndsWith(colGridStr))
                    gltfFile = gltfFile.Remove(gltfFile.Length - colGridStr.Length) + ".gltf";
                else if (gltfFile.EndsWith(colMeshStr))
                    gltfFile = gltfFile.Remove(gltfFile.Length - colMeshStr.Length) + ".gltf";

                if (!dependencies.ContainsKey(gltfFile))
                    dependencies.Add(gltfFile, new List<string>());
                dependencies[gltfFile].Add(localFilePath);
            }
        }

        private void AddDependencies(DirectorySupervisor.Changes changes)
        {
            for (int i = 0; i < changes.Modified.Count; i++)
            {
                if (dependencies.TryGetValue(changes.Modified[i].LocalPath, out List<string>? d))
                {
                    for (int j = 0; j < d.Count; j++)
                    {
                        // if dependent files are already included, remove them so they aren't added two times, and so that the dependent file gets loaded after the dependency
                        var matches = changes.Modified.FindAll(f => f.LocalPath == d[j]).ToArray();
                        for (int k = 0; k < matches.Length; k++)
                            changes.Modified.Remove(matches[k]);

                        // add dependent file
                        changes.Modified.Add(new DirectorySupervisor.FileStamp(Path.Combine(sourceContentDir, d[j]), DateTime.Now, sourceContentDir));
                    }
                }
            }
        }

        /// <summary>
        /// Updates changes. Returns wether something changed.
        /// </summary>
        public bool UpdateChanges()
        {
            // waiting for models finishing exporting
            string modelPath = Path.Combine(sourceContentDir, "Models");
            if (Directory.Exists(modelPath))
            {
                while (Directory.EnumerateFiles(modelPath, "*.exporting", SearchOption.AllDirectories).Any())
                    Thread.Sleep(100);
            }

            var changes = dirSupervisor.GetChanges();

            if (!changes.ModifiedOrCreated().Any() && !changes.Deleted.Any())
                return false;

            AddDependencies(changes);

            if (contentBuilder.Build(changes.ModifiedOrCreated().Select(f => f.Path).ToArray()))//, changes.Deleted.Select(f => f.LocalPath).ToArray());
            {
                foreach (var file in changes.ModifiedOrCreated())
                {
                    Iterate(file, false);
                }
            }

            if (changes.Deleted.Any())
            {
                foreach (var file in changes.Deleted)
                {
                    Iterate(file, true);
                }
            }

            void Iterate(DirectorySupervisor.FileStamp file, bool deleted)
            {
                Type? assetType = ExtensionToAssetType.Convert(file.LocalPath);
                if (assetType == null)
                {
                    if (Path.GetExtension(file.LocalPath) == ".loca")
                        OnTextReload?.Invoke(Path.Combine(TempContentRaw.RootDirectory, file.LocalPath));
                    return;
                }

                ReloadIfLoadedFromType(assetType, file.AssetName, deleted);
            }


            //Game1.ShowPopup(string.Join('\n', 
            //    changes.Modified.Select(f => f.LocalPath + " changed")
            //    .Concat(changes.Created.Select(f => f.LocalPath + " created"))
            //    .Concat(changes.Deleted.Select(f => f.LocalPath + " deleted"))));

            return true;
        }

        public void ReloadIfLoadedFromType(Type assetType, string assetName, bool deleted)
        {
            MethodInfo method = GetType().GetMethod("ReloadIfLoaded")!;
            MethodInfo genericMethod = method.MakeGenericMethod(assetType);
            genericMethod.Invoke(this, new object[] { assetName, deleted });
        }

        public void ReloadIfLoaded<T>(string assetName, bool deleted) where T : class
        {
            AssetHolder<T>? assetHolder = content.GetAssetHolder<T>(assetName);
            if (assetHolder == null) // check if the asset has already been loaded
                return; // if not, then don't bother with replacing

            TempContentRaw.UnloadAsset(assetName); // to dispose

            if (deleted)
            {
                // when deleted, force reload from the base content
                content.ReloadLoadedAsset(assetHolder);
            }
            else
            {
                T? newlyLoadedAsset = TempContentRaw.Load<T>(assetName, new(content, gDevice));
                if (newlyLoadedAsset == null)
                    return;
                assetHolder.Replace(newlyLoadedAsset);
                content.TryTriggerOnLoad(assetName, newlyLoadedAsset!);
                assetHolder.TryTriggerOnLoad();
            }
        }
    }
}
