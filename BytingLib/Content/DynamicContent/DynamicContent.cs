using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public class DynamicContent
    {
        ContentManagerRaw tempContentRaw;
        ContentBuilder contentBuilder;

        DirectorySupervisor dirSupervisor;
        IContentCollector content;

        string sourceContentDir;

        /// <summary>Either localization.csv or any font changed.</summary>
        public event Action OnTextReload;

        public DynamicContent(IServiceProvider serviceProvider, IContentCollector content, string dynamicContentPath = @"..\..\..\Content")
        {
            this.content = content;

            sourceContentDir = Path.GetFullPath(dynamicContentPath);

            if (!Directory.Exists(sourceContentDir))
                throw new DirectoryNotFoundException("directory not found: " + sourceContentDir);

            bool expectEmptyDir;

            if (Path.GetFileName(sourceContentDir) == "Content"
                //&& File.Exists(Path.Combine(sourceContentDir, "Content.mgcb"))
                && File.Exists(Path.Combine(sourceContentDir, "ContentGenerated_do-not-edit.mgcb")))
                expectEmptyDir = false;
            else
                expectEmptyDir = true;

            //sourceContentDir = Paths.ModContent;// Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\Content"));
            //bool expectEmptyDir = true;

            dirSupervisor = new DirectorySupervisor(sourceContentDir, GetFiles, expectEmptyDir);

            string tempPath = Path.Combine(sourceContentDir, "bin", "obj", "Content");
            string tempOutputPath = Path.Combine(sourceContentDir, "bin", "bin", "Content");

            contentBuilder = new ContentBuilder(sourceContentDir, tempOutputPath, tempPath);

            tempContentRaw = new ContentManagerRaw(serviceProvider, contentBuilder.OutputPath);

            if (expectEmptyDir)
                UpdateChanges();
        }

        private string[] GetFiles()
        {
            List<string> files = new List<string>();
            Get("Effects", "*.fx");
            Get("Fonts", "*.xnb|*.spritefont");
            Get("Models", "*.fbx");
            Get("Music", "*.ogg");
            Get("Sounds", "*.ogg|*.wav");
            Get("Textures", "*.png|*.jpg|*.jpeg");
            GetFile("Sounds\\settings.txt");
            GetFile("localization.csv");

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
                if (file.LocalPath.StartsWith("Sounds\\"))
                {
                    throw new NotImplementedException();
                    //if (file.LocalPath == "Sounds\\settings.txt")
                    //{
                    //    // load default settings
                    //    string defaultFile = Path.Combine(content.RootDirectory, file.LocalPath);
                    //    Sounds.LoadSettings(defaultFile);

                    //    // load mod settings
                    //    if (!deleted && defaultFile != file.Path)
                    //        Sounds.LoadSettings(file.Path);
                    //}
                    //else
                    //{
                    //    var val = GetCurrentValue<Sounds>(file);
                    //    if (val is SoundItem sound)
                    //        sound.SoundEffect = Reload<SoundEffect>(file.AssetName, deleted);
                    //}
                }
                else if (file.LocalPath.StartsWith("Textures\\"))
                {
                    var currentValue = content.Seek<Texture2D>(file.AssetName);
                    if (currentValue != null)
                    {
                        var newValue = Reload<Texture2D>(file.AssetName, deleted);
                        if (!deleted)
                        {
                            if (newValue != null)
                            {
                                //if (!currentValue.IsDisposed
                                //    && currentValue.Width == newValue.Width
                                //    && currentValue.Height == newValue.Height)
                                //    currentValue.SetData(newValue.ToColor());
                                //else
                                //{
                                    content.ReplaceAsset(file.AssetName, newValue);
                                //}
                            }
                        }
                    }
                }
                else if (file.LocalPath.StartsWith("Effects\\"))
                {
                    throw new NotImplementedException();
                    //var val = GetCurrentValue<Effects>(file);
                    //if (val is Pointer<Effect> effect)
                    //{
                    //    var parameters = effect.Value.Parameters;

                    //    effect.Value = Reload<Effect>(file.AssetName, deleted);

                    //    CopyParameters(parameters, effect.Value.Parameters);
                    //}
                }
                else if (file.LocalPath == "localization.csv")
                {
                    throw new NotImplementedException();
                    //string defaultFile = Path.Combine(content.RootDirectory, file.LocalPath);

                    //// load mod settings
                    //if (!deleted && defaultFile != file.Path)
                    //    Loca.Reload(file.Path);
                    //else
                    //    Loca.Reload(defaultFile);

                    //textChanged = true;
                }
                else if (file.LocalPath.StartsWith("Fonts\\"))
                {
                    throw new NotImplementedException();
                    //var val = GetCurrentValue<Fonts>(file);
                    //if (val is MyFont font)
                    //    font.ReloadFont(Reload<SpriteFont>(file.AssetName, deleted));

                    //textChanged = true;
                }

                if (onReload.TryGetValue(file.AssetName, out List<Action> actions))
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

        private object GetCurrentValue<AssetContainer>(DirectorySupervisor.FileStamp file)
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

        public T? Reload<T>(string assetName, bool deleted) where T : class
        {
            tempContentRaw.UnloadAsset(assetName); // to dispose and trigger reloading
            if (deleted)
            {
                //return contentRaw.UnloadAsset(assetName);
                return content.ReloadIfLoaded<T>(assetName);
                //return content.Seek<T>(assetName)!;
            }
            else
                return tempContentRaw.Load<T>(assetName);
            //return (deleted ? content : tempContent).Load<T>(assetName);
        }

        Dictionary<string, List<Action>> onReload = new Dictionary<string, List<Action>>();

        /// <summary>Don't forget to unsubscribe.</summary>
        internal void SubscribeToReload(string assetName, Action actionOnReload)
        {
            assetName = assetName.Replace('\\', '/');
            List<Action> actions;
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
            if (onReload.TryGetValue(assetName, out List<Action> actions))
            {
                actions.Remove(actionOnReload);
            }
        }
    }

    //static class Jau
    //{
    //    /// <summary>Copied from newest Monogame ContentManager.UnloadAsset source (added reflection).</summary>
    //    public static bool UnloadAsset(this ContentManager content, string assetName)
    //    {
    //        var loadedAssets = (Dictionary<string, object>)content.GetType().GetField("loadedAssets", System.Reflection.BindingFlags.Instance
    //            | System.Reflection.BindingFlags.NonPublic).GetValue(content);
    //        var disposableAssets = (List<IDisposable>)content.GetType().GetField("disposableAssets", System.Reflection.BindingFlags.Instance
    //            | System.Reflection.BindingFlags.NonPublic).GetValue(content);

    //        assetName = assetName.Replace('\\', '/');

    //        if (string.IsNullOrEmpty(assetName))
    //        {
    //            throw new ArgumentNullException("assetName");
    //        }

    //        //Check if the asset exists
    //        object asset;
    //        if (loadedAssets.TryGetValue(assetName, out asset))
    //        {
    //            //Check if it's disposable and remove it from the disposable list if so
    //            var disposable = asset as IDisposable;
    //            if (disposable != null)
    //            {
    //                disposable.Dispose();
    //                disposableAssets.Remove(disposable);
    //            }

    //            loadedAssets.Remove(assetName);

    //            return true;
    //        }
    //        return false;
    //    }

    //}
}
