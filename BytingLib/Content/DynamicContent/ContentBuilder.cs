using System.Diagnostics;
using System.Text;

namespace BytingLib
{
    class ContentBuilder
    {
        private readonly string modContentDir;
        private readonly string tempOutputPath;
        private readonly string tempPath;

        string header;
        Dictionary<string, CodePart> fileToCode;
        string[] mgcbContents;

        static readonly string mgcbPathExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @".nuget\packages\monogame.content.builder.task\3.8.0.1641\tools\netcoreapp3.1\any\mgcb.exe");
        //@"C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\MGCB.exe";  // this version didn't build my models

        static readonly string outputPath = Path.Combine(Environment.CurrentDirectory, "Content");

        public string OutputPath => tempOutputPath;

        public Action<string> ShowPopup { get; set; } = msg => throw new Exception(msg);

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


        public ContentBuilder(string modContentDir, string tempOutputPath, string tempPath)
        {
            this.modContentDir = modContentDir;
            this.tempOutputPath = tempOutputPath;
            this.tempPath = tempPath;

            Initialize();
        }

        private void Initialize()
        {
            //string cmd = $"/platform:DesktopGL /config: /profile:Reach /compress:False /importer:EffectImporter /processor:EffectProcessor /processorParam:DebugMode=Auto /intermediateDir:\"{tempPath}\" /outputDir:\"{tempOutputPath}\"";

            // get all mgcb files
            string[] mgcbFiles = Directory.GetFiles(outputPath, "*.mgcbcopy");
            mgcbContents = new string[mgcbFiles.Length];

            // check if main content file exists
            int mainContentIndex = Array.IndexOf(mgcbFiles, (Path.Combine(outputPath, "Content.mgcbcopy")));
            if (mainContentIndex == -1)
            {
                mainContentIndex = 0;
                //throw new Exception("Content.mgcb doesn't exist in " + outputPath);
            }

            // read in all mgcb files
            for (int i = 0; i < mgcbFiles.Length; i++)
            {
                mgcbContents[i] = File.ReadAllText(mgcbFiles[i]);
            }

            // get header
            string mainContent = mgcbContents[mainContentIndex];
            int begin = mainContent.IndexOf("#begin");
            header = mainContent.Remove(begin);

            fileToCode = new Dictionary<string, CodePart>();
            for (int i = 0; i < mgcbContents.Length; i++)
            {
                int j = 0;
                while (j != -1 && (j = mgcbContents[i].IndexOf("#begin", j)) != -1)
                {
                    int start = j;
                    int startName = start + "#begin ".Length;
                    int endName = mgcbContents[i].IndexOf("\r\n", startName);
                    int end = mgcbContents[i].IndexOf("#begin", start + 1);
                    string key = mgcbContents[i].Substring(startName, endName - startName);
                    fileToCode.Add(key, new CodePart(i, start, end == -1 ? -1 : end - start));
                    j = end;
                }
            }
        }

        public bool Build(string[] changes)
        {
            if (changes.Length == 0)
                return false;

            if (Directory.Exists(tempOutputPath))
                Directory.Delete(tempOutputPath, true);

            string cmd = header;

            // set temporary directories
            cmd += $"/intermediateDir:{tempPath}\r\n/outputDir:{tempOutputPath}\r\n\r\n";

            foreach (var file in changes)
            {
                string localFile = Path.GetRelativePath(modContentDir, file).Replace('\\', '/');
                if (fileToCode.TryGetValue(localFile, out CodePart code))
                {
                    cmd += code.GetCode(mgcbContents);
                }
            }

            string contentTempFile = Path.Combine(modContentDir, "Content.mgcbtemp");

            File.WriteAllText(contentTempFile, cmd);

            string newCmd = "/@:\"" + contentTempFile + "\"";

            Process process = new Process
            {
                StartInfo =
                {
                    FileName = mgcbPathExe,
                    Arguments = newCmd,
                    CreateNoWindow = true,
                    WorkingDirectory = modContentDir,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            //Get program output
            string stdError = null;
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
                ShowPopup("DynamicContent Error: " + e.Message);
            }

            return true;
        }

        private static bool CheckOutput(StringBuilder stdOutput, Action<string> showPopup)
        {
            var str = stdOutput.ToString();
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
    }
}
