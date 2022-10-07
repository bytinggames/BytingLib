
namespace BytingLib
{
    class DirectorySupervisor
    {
        private readonly string directory;
        private readonly Func<string[]> getFiles;

        public class FileStamp
        {
            public string Path { get; }
            private int supervisorDirLength;
            public string LocalPath => Path.Substring(supervisorDirLength);
            public string AssetName
            {
                get
                {
                    string path = LocalPath;
                    Type? t = ExtensionToAssetType.Convert(path);
                    if (t != typeof(string)
                        && t != typeof(AnimationData)) // string assets keep their extensions
                        path = path.Remove(path.Length - System.IO.Path.GetExtension(path).Length); // remove extension
                    return path.Replace('\\', '/');
                }
            }
            public DateTime LastChange { get; }

            public FileStamp(string path, DateTime lastChange, string supervisorDir)
            {
                Path = path;
                LastChange = lastChange;

                supervisorDirLength = supervisorDir.Length;
                if (!supervisorDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    supervisorDirLength++;
            }
        }

        public class Changes
        {
            public List<FileStamp> Modified = new List<FileStamp>();
            public List<FileStamp> Created = new List<FileStamp>();
            public List<FileStamp> Deleted = new List<FileStamp>();

            public IEnumerable<FileStamp> ModifiedOrCreated()
            {
                foreach (var m in Modified)
                {
                    yield return m;
                }

                foreach (var c in Created)
                {
                    yield return c;
                }
            }
        }

        class RememberedFile
        {
            public long FileSize { get; set; }

            public RememberedFile(long fileSize)
            {
                FileSize = fileSize;
            }
        }

        Dictionary<string, RememberedFile> files;

        DateTime lastUpdate;

        const string searchPattern = "*";

        public DirectorySupervisor(string directory, Func<string[]> getFiles, bool expectEmptyDirectory)
        {
            this.directory = directory;
            this.getFiles = getFiles;

            if (expectEmptyDirectory)
            {
                lastUpdate = DateTime.MinValue;
                files = new Dictionary<string, RememberedFile>();
            }
            else
            {
                lastUpdate = DateTime.Now;
                files = GetFilesDictionary();
            }
        }

        private Dictionary<string, RememberedFile> GetFilesDictionary()
        {
            var files = new Dictionary<string, RememberedFile>();

            foreach (var file in GetFiles())
            {
                files.Add(file, new RememberedFile(new FileInfo(file).Length));
            }

            return files;
        }

        private string[] GetFiles()
        {
            if (getFiles == null)
                return Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
            else
                return getFiles();
        }

        public Changes GetChanges()
        {
            Changes changes = new Changes();

            var cFiles = GetFiles();

            DateTime newestUpdate = DateTime.MinValue;

            HashSet<string> oldFiles = new HashSet<string>(files.Keys.ToList());

            foreach (var cFile in cFiles)
            {
                // get latest access or write time
                var tWrite = File.GetLastWriteTime(cFile);

                if (tWrite > lastUpdate)
                {
                    FileChanged(tWrite, new FileInfo(cFile).Length);
                }
                else
                {
                    var tAccess = File.GetLastAccessTime(cFile);
                    if (tAccess > lastUpdate)
                    {
                        long fileSize = new FileInfo(cFile).Length;
                        if (!files.ContainsKey(cFile)
                            || files[cFile].FileSize != fileSize)
                        {
                            FileChanged(tAccess, fileSize);
                        }
                    }
                }

                void FileChanged(DateTime changeTime, long fileSize)
                {
                    if (changeTime > newestUpdate)
                        newestUpdate = changeTime;

                    FileStamp fileStamp = new FileStamp(cFile, changeTime, directory);

                    RememberedFile? rememberedFile;

                    if (files.TryGetValue(cFile, out rememberedFile))
                    {
                        changes.Modified.Add(fileStamp);
                        rememberedFile.FileSize = fileSize;
                    }
                    else
                    {
                        changes.Created.Add(fileStamp);
                        files.Add(cFile, new RememberedFile(fileSize));
                    }
                }

                oldFiles.Remove(cFile);
            }

            DateTime now = DateTime.Now;
            foreach (var lostFile in oldFiles)
            {
                FileStamp fileStamp = new FileStamp(lostFile, now, directory);
                changes.Deleted.Add(fileStamp);
                files.Remove(lostFile);
            }

            //files = cFiles;
            if (newestUpdate > lastUpdate)
                lastUpdate = newestUpdate;

            return changes;
        }

        public string[] GetExistingFilesFromLastCheck()
        {
            return files.Keys.ToArray();
        }
    }
}
