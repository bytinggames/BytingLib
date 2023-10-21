namespace BytingLib
{
    public static class MGCBParser
    {
        public static void Apply(List<MGCBItem> actions, ContentConverter contentConverter)
        {
            foreach (var action in actions)
            {
                if (action.CSharpType == null)
                {
                    if (action.Processor != null)
                    {
                        if (!contentConverter.ProcessorToDataType.TryGetValue(action.Processor, out action.CSharpType))
                        {
                            throw new BytingException("no data type specified for processor " + action.Processor + " for building " + action.FilePath + ".");
                        }
                    }
                    else
                    {
                        if (!contentConverter.ExtensionCopyToDataType.TryGetValue(Path.GetExtension(action.FilePath).Substring(1), out action.CSharpType))
                        {
                            throw new BytingException("no csharp type specified for copying " + action.FilePath + ". Specify it via #/c#type:<Type> or pass via BuildTemplates.dll arguments");
                        }
                    }
                }

                if (action.CSharpVarName == null)
                {
                    contentConverter.DataTypeToVarExtension.TryGetValue(action.CSharpType, out action.CSharpVarName);
                }

                action.CSharpVarName ??= action.CSharpType;
            }
        }

        public static List<MGCBItem> GetMGCBActions(string mgcbOutput)
        {
            List<MGCBItem> contentActions = new();

            ScriptReader reader = new ScriptReader("\n" /* for recognizing \n/ command */ + mgcbOutput.Replace("\r", ""));

            string filePath;
            string? processor = null;
            string? cSharpType = null;
            string? cSharpVarName = null;
            HashSet<string> filePaths = new();

            try
            {
                while (true)
                {
                    reader.ReadToStringOrEnd(new string[] { "\n/", "\n#/" }, out int reachedStringIndex);
                    if (reachedStringIndex == -1)
                    {
                        break;
                    }

                    string command = reader.ReadToChar(':');
                    if (reachedStringIndex == 0)
                    {
                        if (command == "processor")
                        {
                            processor = reader.ReadToChar('\n');
                        }
                    }
                    else if (reachedStringIndex == 1) // custom non-mgcb commands
                    {
                        if (command == "c#type") // #/c#type:SpriteFont
                        {
                            cSharpType = reader.ReadToChar('\n');
                        }

                        if (command == "c#name")
                        {
                            cSharpVarName = reader.ReadToChar('\n');
                        }
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
                            {
                                processor = null;
                            }
                            else /* build */ if (processor == null)
                            {
                                throw new BytingException("no processor set for building " + filePath);
                            }

                            contentActions.Add(new MGCBItem(filePath, processor, cSharpType, cSharpVarName));
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

            return contentActions;
        }

        private static string GetFilePath(ScriptReader reader)
        {
            string assetName = reader.ReadToChar('\n');
            int colonIndex = assetName.IndexOf(';');
            if (colonIndex != -1)
            {
                assetName = assetName.Substring(colonIndex + 1);
            }

            return assetName;
        }
    }
}
