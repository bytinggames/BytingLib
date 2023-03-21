using System.Reflection;

namespace BytingLib
{
    public class HotReloadContent
    {
        public ContentManagerRaw TempContentRaw { get; }
        ContentBuilder contentBuilder;

        DirectorySupervisor dirSupervisor;
        IContentCollector content;
        private readonly ContentConverter contentConverter;
        GraphicsDevice gDevice;

        string sourceContentDir;

        /// <summary>Either localization.csv or any font changed.</summary>
        public event Action<string>? OnTextReload;

        readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        public HotReloadContent(IServiceProvider serviceProvider, IContentCollector content, string hotReloadContentPath, ContentConverter contentConverter)
        {
            this.content = content;
            this.contentConverter = contentConverter;
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

            contentBuilder = new ContentBuilder(sourceContentDir, tempOutputPath, tempPath, contentConverter);

            TempContentRaw = new ContentManagerRaw(serviceProvider, contentBuilder.OutputPath);

            gDevice = (serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService)!.GraphicsDevice;

            if (expectEmptyDir)
                UpdateChanges();
        }

        private string[] GetFiles()
        {
            // get all files from top directory
            List<string> files = Directory.GetFiles(sourceContentDir, "*.*", SearchOption.TopDirectoryOnly).ToList();

            // get all files from subdirectories, excluding bin and obj
            string[] topDirectories = Directory.GetDirectories(sourceContentDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var topDir in topDirectories)
            {
                if (!topDir.EndsWith("\\bin") && !topDir.EndsWith("/bin")
                     && !topDir.EndsWith("\\obj") && !topDir.EndsWith("/obj"))
                    files.AddRange(Directory.GetFiles(topDir, "*.*", SearchOption.AllDirectories));
            }


            dependencies.Clear();
            InitEffectDependencies(files);
            InitGLTFDependencies(files);

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

            List<MGCBItem> itemsChangedOrCreated;
            List<MGCBItem> itemsDeleted;
            if (contentBuilder.Build(changes.ModifiedOrCreated().Select(f => f.Path).ToArray(),
                changes.Deleted.Select(f => f.Path).ToArray(),
                out itemsChangedOrCreated,
                out itemsDeleted))
            {
                Update(itemsChangedOrCreated, false);
            }

            Update(itemsDeleted, true);

            void Update(List<MGCBItem> items, bool deleted)
            {
                foreach (var file in items)
                {
                    string assetName = file.FilePath;
                    int lastDotIndex = assetName.LastIndexOf('.');
                    // remove extension
                    if (lastDotIndex != -1)
                        assetName = assetName.Remove(lastDotIndex);

                    if (file.CSharpType != null
                        && contentConverter.RuntimeTypes.TryGetValue(file.CSharpType, out Type? dataType))
                        ReloadFromTypeIfLoaded(dataType, assetName, deleted);
                }
            }

            //Game1.ShowPopup(string.Join('\n', 
            //    changes.Modified.Select(f => f.LocalPath + " changed")
            //    .Concat(changes.Created.Select(f => f.LocalPath + " created"))
            //    .Concat(changes.Deleted.Select(f => f.LocalPath + " deleted"))));

            return true;
        }

        public void ReloadFromTypeIfLoaded(Type assetType, string assetName, bool deleted)
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
