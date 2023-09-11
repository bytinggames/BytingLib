using System.Diagnostics;
using System.Text;

namespace BytingLib
{
    public class ContentBuilder
    {
        private readonly string modContentDir;
        private readonly string tempOutputPath;
        private readonly string tempPath;
        private readonly ContentConverter contentConverter;
        private readonly string header = "";
        private readonly Dictionary<string, CodePart> fileToCode = new Dictionary<string, CodePart>();
        private readonly string[] mgcbContents = new string[0];

        static readonly string outputPath = Path.Combine(Environment.CurrentDirectory, "Content");

        public string OutputPath => tempOutputPath;

        public Action<string> ShowPopup { get; set; } = msg => throw new Exception(msg);

        /// <summary>This is disabled per default, so hot reloaded content will stay built, even when other content files are getting built afterwards.</summary>
        public bool DeleteTempOutputDirectoryBeforeBuild { get; set; } = false;

        struct CodePart
        {
            int contentIndex, start, length;

            public CodePart(int contentIndex, int start, int length)
            {
                this.contentIndex = contentIndex;
                this.start = start;
                this.length = length;
            }

            public string GetCode(string[] mgcbContents)
            {
                if (length == -1)
                    return mgcbContents[contentIndex].Substring(start);
                else
                    return mgcbContents[contentIndex].Substring(start, length);
            }
        }


        public ContentBuilder(string modContentDir, string tempOutputPath, string tempPath, ContentConverter contentConverter)
        {
            this.modContentDir = modContentDir;
            this.tempOutputPath = tempOutputPath;
            this.tempPath = tempPath;
            this.contentConverter = contentConverter;

            // get all mgcb files
            string[] mgcbFiles;
            if (Directory.Exists(outputPath))
                mgcbFiles = Directory.GetFiles(outputPath, "*.mgcbcopy");
            else
                return;

            mgcbContents = new string[mgcbFiles.Length];

            // check if main content file exists
            int mainContentIndex = Array.IndexOf(mgcbFiles, (Path.Combine(outputPath, "Content.Generated.mgcbcopy")));
            if (mainContentIndex == -1)
            {
                mainContentIndex = 0;
                //throw new Exception("Content.Generated.mgcb doesn't exist in " + outputPath);
            }

            // read in all mgcb files
            for (int i = 0; i < mgcbFiles.Length; i++)
            {
                mgcbContents[i] = File.ReadAllText(mgcbFiles[i]);
            }

            // get header
            string mainContent = mgcbContents[mainContentIndex];
            int begin = mainContent.IndexOf("#begin");
            if (begin == -1)
                begin = mainContent.Length;

            // define variables (match Targets.targets: <!-- Customizing the Content Build Process --> )

            string Configuration, RuntimeIdentifier;
#if DEBUG
            Configuration = "Debug";
#else
            Configuration = "Release";
#endif

#if WINDOWS
            RuntimeIdentifier = "win-x64";
#elif LINUX
            RuntimeIdentifier = "linux-x64";
#elif OSX
            RuntimeIdentifier = "osx-x64";
#else
            throw new Exception("Not sure which OS this is running on");
#endif

            header = @$"
/define:{Configuration}
/define:Configuration={Configuration}
/define:RuntimeIdentifier={RuntimeIdentifier}
/define:ConfigRuntime={Configuration}_{RuntimeIdentifier}
";

            string sourceHeader = mainContent.Remove(begin);
            sourceHeader = AdaptReferences(sourceHeader);

            header += sourceHeader;

            for (int i = 0; i < mgcbContents.Length; i++)
            {
                int j = 0;
                while (j != -1 && (j = mgcbContents[i].IndexOf("#begin", j)) != -1)
                {
                    int start = j;
                    int startName = start + "#begin ".Length;
                    int endName = mgcbContents[i].IndexOf("\n", startName);
                    if (mgcbContents[i][endName - 1] == '\r') // check if it's a \r\n
                        endName--;
                    string key = mgcbContents[i].Substring(startName, endName - startName);
                    int end = mgcbContents[i].IndexOf("#begin", start + 1);
                    fileToCode.Add(key, new CodePart(i, start, end == -1 ? -1 : end - start));
                    j = end;
                }
            }
        }

        private static string AdaptReferences(string sourceHeader)
        {

            // alter references, so that they point to the directory of the exe (Environment.CurrentDirectory)
            // you need to supply that directory with all the required content dlls
            int referenceIndex = 0;
            const string refStr = "/reference:";
            while ((referenceIndex = sourceHeader.IndexOf(refStr, referenceIndex)) != -1)
            {
                referenceIndex += refStr.Length;
                int endLineIndex = sourceHeader.IndexOf('\n', referenceIndex);
                if (endLineIndex != -1)
                {
                    string reference = sourceHeader.Substring(referenceIndex, endLineIndex - referenceIndex).Replace('\\', '/');
                    int lastSlashIndex = reference.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                        reference = reference.Substring(lastSlashIndex + 1);
                    string referenceDll = Environment.CurrentDirectory + "/" + reference;
                    sourceHeader = sourceHeader.Remove(referenceIndex) + referenceDll + sourceHeader.Substring(endLineIndex);
                    referenceIndex += referenceDll.Length;
                }
            }

            return sourceHeader;
        }

        public bool Build(string[] changes, string[] deleted, out List<MGCBItem> itemsChanged, out List<MGCBItem> itemsDeleted)
        {
            itemsChanged = new();
            itemsDeleted = new();

            if (changes.Length == 0 && deleted.Length == 0)
                return false;

            if (DeleteTempOutputDirectoryBeforeBuild)
            {
                if (Directory.Exists(tempOutputPath))
                    Directory.Delete(tempOutputPath, true);
            }

            string cmd = header;

            // set temporary directories
            cmd += $"/intermediateDir:{tempPath}\r\n/outputDir:{tempOutputPath}\r\n\r\n";

            bool anyTasksAdded = false;

            foreach (var file in changes)
            {
                string localFile = Path.GetRelativePath(modContentDir, file).Replace('\\', '/');
                if (fileToCode.TryGetValue(localFile, out CodePart code))
                {
                    string addCode = code.GetCode(mgcbContents);
                    cmd += addCode;
                    anyTasksAdded = true;

                    var mgcbActions = MGCBParser.GetMGCBActions(addCode);
                    MGCBParser.Apply(mgcbActions, contentConverter);
                    itemsChanged.AddRange(mgcbActions);
                }
            }

            foreach (var file in deleted)
            {
                string localFile = Path.GetRelativePath(modContentDir, file).Replace('\\', '/');
                if (fileToCode.TryGetValue(localFile, out CodePart code))
                {
                    string addCode = code.GetCode(mgcbContents);

                    var mgcbActions = MGCBParser.GetMGCBActions(addCode);
                    MGCBParser.Apply(mgcbActions, contentConverter);
                    itemsDeleted.AddRange(mgcbActions);
                }
            }

            if (!anyTasksAdded)
                return false; // not necessary to build, skip it

            string contentTempFile = Path.Combine(modContentDir, "Content.mgcb.tmp");

            File.WriteAllText(contentTempFile, cmd);

            string command = "dotnet mgcb-byting /@:\"" + contentTempFile + "\"";
            string fileName;
#if WINDOWS
            command = "/C " + command;
            fileName = "cmd.exe";
#else
            command = command.Replace("\"", "\"\"");
            command = "-c  \"" + command + "\"";
            fileName = "/bin/bash";
#endif

            Process process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = command,
                    CreateNoWindow = true,
                    WorkingDirectory = modContentDir,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };


            //Get program output
            string? stdError = null;
            StringBuilder stdOutput = new StringBuilder();
            process.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!CheckOutput(stdOutput, ShowPopup))
                    return false;
            }
            catch (Exception e)
            {
                ShowPopup("HotReloadContent Error: " + e.ToString() + "\n\n" + "build output error: " + stdError);
            }

            return true;
        }

        private static bool CheckOutput(StringBuilder stdOutput, Action<string> showPopup)
        {
            var str = stdOutput.ToString();
            try
            {
                string success = " succeeded, ";
                int successCountEnd = str.IndexOf(success);
                int successCountStart = str.LastIndexOf(' ', successCountEnd - 1, successCountEnd - 1) + 1;
                string successCountStr = str.Substring(successCountStart, successCountEnd - successCountStart);
                int successCount;
                if (!int.TryParse(successCountStr, out successCount))
                {
                    showPopup("couldn't parse " + str);
                }

                int indexStart = str.IndexOf(success) + success.Length;
                int indexEnd = str.IndexOf(" failed.Time elapsed");
                string failedNumberStr = str.Substring(indexStart, indexEnd - indexStart);
                int failedNumber = int.Parse(failedNumberStr);

                string errorStr = ": error ";
                int errorIndex = str.IndexOf(errorStr) + 2;

                if (failedNumber > 0)
                {
                    showPopup(str.Substring(errorIndex));
                    return false;
                }
                return successCount > 0;
            }
            catch (Exception e)
            {
                throw new BytingException("Error in ContentBuilder.CheckOutput(). Build output is: \n" + str, e); 
            }
        }
    }
}
