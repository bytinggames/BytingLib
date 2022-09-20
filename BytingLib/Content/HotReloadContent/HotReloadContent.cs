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
        public event Action? OnTextReload;

        readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        public HotReloadContent(IServiceProvider serviceProvider, IContentCollector content, string directoryContainingMonoGame, string hotReloadContentPath = @"..\..\..\Content")
        {
            this.content = content;

            sourceContentDir = Path.GetFullPath(hotReloadContentPath);

            bool expectEmptyDir;

            if (Path.GetFileName(sourceContentDir) == "Content"
                && Directory.EnumerateFiles(sourceContentDir, "*.mgcb", SearchOption.TopDirectoryOnly).Any()) // check if any mgcb file is present
                expectEmptyDir = false;
            else
                expectEmptyDir = true;

            //sourceContentDir = Paths.ModContent;// Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\Content"));
            //bool expectEmptyDir = true;

            dirSupervisor = new DirectorySupervisor(sourceContentDir, GetFiles, expectEmptyDir);

            string tempPath = Path.Combine(sourceContentDir, "obj", "HotReload", "Content");
            string tempOutputPath = Path.Combine(sourceContentDir, "bin", "HotReload", "Content");

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            if (Directory.Exists(tempOutputPath))
                Directory.Delete(tempOutputPath, true);

            contentBuilder = new ContentBuilder(sourceContentDir, tempOutputPath, tempPath, directoryContainingMonoGame);

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
            Get("Textures", "*.png|*.jpg|*.jpeg|*.json");
            GetFile("Sounds\\settings.txt");
            GetFile("localization.csv");

            AddEffectDependencies(files);

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

        private void AddEffectDependencies(List<string> files)
        {
            // add effect dependencies (fx depend on fxh's)
            const string includeStr = "#include \"";
            foreach (var f in files)
            {
                string localFilePath = f.Substring(sourceContentDir.Length + 1);

                string shaderCode = File.ReadAllText(f);
                int i = 0;
                while ((i = shaderCode.IndexOf(includeStr, i)) != -1)
                {
                    i += includeStr.Length;
                    int i2 = shaderCode.IndexOf("\"", i);
                    string file = shaderCode.Substring(i, i2 - i);
                    file = "Effects\\" + file;
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

        public void UpdateChanges()
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
                return;

            AddDependencies(changes);

            bool textChanged = false;

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

            if (textChanged)
                OnTextReload?.Invoke();

            void Iterate(DirectorySupervisor.FileStamp file, bool deleted)
            {
                if (file.LocalPath == "localization.csv")
                {
                    if (!deleted)
                        Loca.Reload(file.Path);
                    // TODO: when mod localization file is deleted, the default localization file should be loaded again
                    return;
                }

                Type? assetType = ExtensionToAssetType.Convert(file.LocalPath);
                if (assetType == null)
                    return;

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
        }

        private object? GetCurrentValue<AssetContainer>(DirectorySupervisor.FileStamp file)
        {
            string name = Path.GetFileNameWithoutExtension(file.Path);
            var field = typeof(AssetContainer).GetField(name);
            if (field == null)
                return null;

            return field.GetValue(null);
        }

        private void CopyParameters(EffectParameterCollection parametersSource, EffectParameterCollection parametersTarget)
        {
            foreach (var p in parametersSource)
            {
                var p2 = parametersTarget[p.Name];
                if (p2 == null) // removed parameter
                    continue;
                switch (p.ParameterType)
                {
                    case EffectParameterType.Bool:
                        p2.SetValue(p.GetValueBoolean());
                        break;
                    case EffectParameterType.Int32:
                        if (p.Elements.Count == 0)
                            p2.SetValue(p.GetValueInt32());
                        else
                            p2.SetValue(p.GetValueInt32Array());
                        break;
                    case EffectParameterType.Single:
                        if (p.Elements.Count == 0)
                        {
                            if (p.ColumnCount == 1 && p.RowCount == 1)
                                p2.SetValue(p.GetValueSingle());
                            else if (p.RowCount == 1)
                            {
                                switch (p.ColumnCount)
                                {
                                    case 2:
                                        p2.SetValue(p.GetValueVector2());
                                        break;
                                    case 3:
                                        p2.SetValue(p.GetValueVector3());
                                        break;
                                    case 4:
                                        p2.SetValue(p.GetValueVector4());
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            else
                                p2.SetValue(p.GetValueMatrix());
                        }
                        else
                        {
                            if (p.ColumnCount == 1 && p.RowCount == 1)
                                p2.SetValue(p.GetValueSingleArray());
                            else if (p.RowCount == 1)
                            {
                                switch (p.ColumnCount)
                                {
                                    case 2:
                                        p2.SetValue(p.GetValueVector2Array());
                                        break;
                                    case 3:
                                        p2.SetValue(p.GetValueVector3Array());
                                        break;
                                    case 4:
                                        p2.SetValue(p.GetValueVector4Array());
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            else
                                p2.SetValue(p.GetValueMatrixArray(p.Elements.Count));
                        }
                        break;
                    case EffectParameterType.Texture:
                    case EffectParameterType.Texture2D:
                        p2.SetValue(p.GetValueTexture2D());
                        break;
                    case EffectParameterType.Texture3D:
                        p2.SetValue(p.GetValueTexture3D());
                        break;
                    case EffectParameterType.TextureCube:
                        p2.SetValue(p.GetValueTextureCube());
                        break;
                    case EffectParameterType.Texture1D:
                    default:
                        throw new NotImplementedException();
                    case EffectParameterType.Void:
                        break;
                    case EffectParameterType.String:
                        // there is no set string...
                        //p2.SetValue(p.GetValueString());
                        break;
                        //p2.SetValue(p.GetValueQuaternion()); // I think this is the same as Vector4, so no need to read it separately
                }
            }
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
            }
        }

        Dictionary<string, List<Action>> onReload = new Dictionary<string, List<Action>>();

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
