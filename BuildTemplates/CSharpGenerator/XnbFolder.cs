namespace BuildTemplates
{
    public class XnbFolder
    {
        private readonly string name;
        private readonly string className;
        private readonly List<Xnb> files = new();
        private readonly List<XnbFolder> folders = new();

        const string endl = "\r\n";
        public const string tab = "    ";

        public XnbFolder(string folderName, string? customClassName, List<Xnb> xnbs)
        {
            if (folderName.Length > 0)
            {
                name = Xnb.ToVariableName(folderName);
            }
            else
            {
                name = "";
            }
            className = customClassName ?? "_" + name;

            if (folderName.Length > 0)
            {
                for (int i = 0; i < xnbs.Count; i++)
                {
                    xnbs[i].FileName = xnbs[i].FileName.Substring(folderName.Length + 1);
                }
            }

            for (int i = 0; i < xnbs.Count; i++)
            {
                int slashIndex = xnbs[i].FileName.IndexOf('/');
                if (slashIndex != -1)
                {
                    string newFolderName = xnbs[i].FileName.Remove(slashIndex);
                    List<Xnb> xnbsForNewFolder = new();
                    for (int j = 0; j < xnbs.Count; j++)
                    {
                        if (xnbs[j].FileName.StartsWith(newFolderName + "/"))
                        {
                            xnbsForNewFolder.Add(xnbs[j]);
                            xnbs.RemoveAt(j--);
                        }
                    }
                    folders.Add(new XnbFolder(newFolderName, null, xnbsForNewFolder));
                    i--; // cause current file turned into a folder
                }
                else
                {
                    files.Add(xnbs[i]);
                }
            }
        }

        public override string ToString()
        {
            return name;
        }

        public string Print(string contentDirectory, string tabs, bool loadOnStartup)
        {
            if (!string.IsNullOrEmpty(contentDirectory))
            {
                contentDirectory += "/";
            }

            string contentLoaderClass = "";
            string folderProperties = "";
            string assets = "";
            string assets2 = "";
            string classes = "";
            string path = endl + tab + $"public const string Path = \"{contentDirectory}\";";

            if (className == "ContentLoader")
            {
                contentLoaderClass = $@"
{tab}private readonly RefLoaderDependencies d = d;
{tab}public Ref<T> Use<T>(string assetPath)
{tab}{{
{tab}{tab}return d.Use<T>(assetPath);
{tab}}}
{tab}public void Override<T>(string assetPath, Ref<T> assetRef)
{tab}{{
{tab}{tab}d.Override<T>(assetPath, assetRef);
{tab}}}";
            }

            foreach (var folder in folders)
            {
                folderProperties += endl + tab + $"public _{folder.name} {folder.name} {{ get; }} = new _{folder.name}(d);";
                classes += endl + tab + folder.Print(contentDirectory + folder.name, tabs, loadOnStartup);
            }

            foreach (var file in files)
            {
                string? print = file.Print(loadOnStartup, tab);
                assets += endl + tab + print;
                print = file.PrintRefLoader(loadOnStartup, tab);
                assets2 += endl + tab + print;
            }

            string output = $@"public class {className}(RefLoaderDependencies d)
{{{contentLoaderClass}{path}{assets}{assets2}{folderProperties}{classes}
}}";
            return output.Replace("\r\n", "\n") // make consistent among OSs
                .Replace("\n", "\n" + tabs); // indent
        }

    }
}
