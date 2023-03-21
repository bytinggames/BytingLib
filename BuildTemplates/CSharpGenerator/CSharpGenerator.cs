using BytingLib;

namespace BuildTemplates
{
    static class CSharpGenerator
    {
        internal static XnbFolder FromMGCB(string mgcbOutput, Dictionary<string, string> processorToDataType, Dictionary<string, string> dataTypeToNameExtension, Dictionary<string, string> extensionCopyToDataType, bool loadOnStartup)
        {
            ScriptReader reader = new ScriptReader("\n" /* for recognizing \n/ command */ + mgcbOutput.Replace("\r", ""));

            List<Xnb> xnbs = new List<Xnb>();

            string? processor = null;
            string filePath;
            string? cSharpType = null;
            string? cSharpVarName = null;
            HashSet<string> filePaths = new();

            try
            {
                while (true)
                {
                    reader.ReadToStringOrEnd(new string[] { "\n/", "\n#/" }, out int reachedStringIndex);
                    if (reachedStringIndex == -1)
                        break;
                    string command = reader.ReadToChar(':');
                    if (reachedStringIndex == 0)
                    {
                        if (command == "processor")
                            processor = reader.ReadToChar('\n');
                    }
                    else if (reachedStringIndex == 1) // custom non-mgcb commands
                    {
                        if (command == "c#type") // #/c#type:SpriteFont
                            cSharpType = reader.ReadToChar('\n');
                        if (command == "c#name")
                            cSharpVarName = reader.ReadToChar('\n');
                    }

                    if (command == "copy" || command == "build")
                    {
                        bool copy = command == "copy";
                        filePath = GetFilePath(reader);
                        if (filePath.Contains("json"))
                        { }
                        if (filePaths.Add(filePath)) // don't allow duplicates
                        {
                            if (copy)
                                processor = null;
                            else /* build */ if (processor == null)
                                throw new BytingException("no processor set for building " + filePath);

                            string cSharpDataType;

                            if (cSharpType != null)
                                cSharpDataType = cSharpType;
                            else
                            {
                                if (processor != null)
                                {
                                    if (processorToDataType.TryGetValue(processor, out string? dataType))
                                        cSharpDataType = dataType;
                                    else
                                        throw new BytingException("no data type specified for processor " + processor + " for building " + filePath + ".");
                                }
                                else
                                {
                                    if (extensionCopyToDataType.TryGetValue(Path.GetExtension(filePath).Substring(1), out cSharpType))
                                        cSharpDataType = cSharpType;
                                    else
                                        throw new BytingException("no csharp type specified for copying " + filePath + ". Specify it via #/c#type:<Type> or pass via BuildTemplates.dll arguments");
                                }
                            }

                            if (cSharpVarName == null)
                                dataTypeToNameExtension.TryGetValue(cSharpDataType, out cSharpVarName);

                            Xnb xnb = new Xnb(filePath, cSharpDataType, cSharpVarName ?? cSharpDataType, loadOnStartup);
                            xnbs.Add(xnb);
                        } 

                        processor = null;
                        cSharpType = null;
                        cSharpVarName = null;
                    }
                    reader.Move(-1); // move before \n
                }
            }
            catch (ScriptReaderException)
            {
                // end of file reached
            }


            XnbFolder root = new XnbFolder("", "ContentLoader", xnbs);

            return root;
        }

        private static string GetFilePath(ScriptReader reader)
        {
            string assetName = reader.ReadToChar('\n');
            int colonIndex = assetName.IndexOf(';');
            if (colonIndex != -1)
                assetName = assetName.Substring(colonIndex + 1);
            return assetName;
        }
    }
}
