using BytingLib;
using System.Collections.Generic;

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
            this.name = folderName;
            className = customClassName ?? "_" + folderName;

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
                contentDirectory += "/";

            string folderProperties = "";
            string folderConstruct = "";
            string assets = "";
            string classes = "";
            string fieldInitialize = "";


            foreach (var folder in folders)
            {
                folderProperties += endl + tab + $"public _{folder.name} {folder.name} {{ get; }}";
                folderConstruct += endl + tab + tab + $"{folder.name} = new _{folder.name}(collector, disposables);";
                classes += endl + tab + folder.Print(contentDirectory + folder.name, tabs, loadOnStartup);
            }

            foreach (var file in files)
            {
                string? print = file.PrintDeclare(loadOnStartup);
                if (print == null)
                    continue;
                assets += endl + tab + print;

                print = file.PrintInit(loadOnStartup);
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

    }
}
