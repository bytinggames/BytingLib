using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace BytingLib
{
    public class HotReloadContent
    {
        public ContentManagerRaw TempContentRaw { get; }
        ContentBuilder contentBuilder;

        DirectorySupervisor dirSupervisor;
        IContentCollector content;

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

            if (expectEmptyDir)
                UpdateChanges();
        }

        private string[] GetFiles()
        {
            List<string> files = new List<string>();
            Get("Effects", "*.fx|*.fxh");
            Get("Fonts", "*.xnb|*.spritefont");
            Get("Models", "*.fbx");
            Get("Music", "*.ogg");
            Get("Sounds", "*.ogg|*.wav");
            Get("Textures", "*.png|*.jpg|*.jpeg|*.ani");
            Get("", "*.txt|*.csv|*.json|*.xml|*.ini|*.config");
            GetFile("Loca.loca");

            InitEffectDependencies(files);

            void Get(string folder, string searchPattern)
            {
                if (!Directory.Exists(Path.Combine(sourceContentDir, folder)))
                    return;
                string[] searchPatterns = searchPattern.Split('|');
                foreach (string sp in searchPatterns)
                    files.AddRange(Directory.GetFiles(Path.Combine(sourceContentDir, folder), sp, SearchOption.AllDirectories));
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
            dependencies.Clear();
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
                    file = Path.Combine("Effects", file);
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


                if (onReload.TryGetValue(file.AssetName, out List<Action>? actions))
                {
                    if (actions != null)
                    {
                        var copy = new List<Action>(actions);
                        for (int i = 0; i < copy.Count; i++)
                        {
                            copy[i]?.Invoke();
                        }
                    }
                }
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
                T? newlyLoadedAsset = TempContentRaw.Load<T>(assetName);
                if (newlyLoadedAsset == null)
                    return;
                assetHolder.Replace(newlyLoadedAsset);
                content.TryTriggerOnLoad(assetName, newlyLoadedAsset!);
            }
        }

        Dictionary<string, List<Action>> onReload = new Dictionary<string, List<Action>>(); // TODO: remove this, cause redundant, cause already in ContentCollector?

        /// <summary>Don't forget to unsubscribe.</summary>
        internal void SubscribeToReload(string assetName, Action actionOnReload)
        {
            assetName = assetName.Replace('\\', '/');
            List<Action>? actions;
            if (!onReload.TryGetValue(assetName, out actions))
            {
                actions = new List<Action>();
                onReload.Add(assetName, actions);
            }
            actions.Add(actionOnReload);
        }
        internal void UnsubscribeToReload(string assetName, Action actionOnReload)
        {
            assetName = assetName.Replace('\\', '/');
            if (onReload.TryGetValue(assetName, out List<Action>? actions))
            {
                actions.Remove(actionOnReload);
            }
        }
    }
}
