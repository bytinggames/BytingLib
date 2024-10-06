internal class Program
{
    enum ActionType
    {
        None,
        AddProperty,
        AddConstant,
        RemoveProperty,
        RemoveConstant
    }

    const string usage = "\nuse like SetGlobalProperties.exe \"path/to/GlobalProps.props\" -AddProperty Demo -AddConstant DEMO -RemoveProperty Test -RemoveConstant TEST";

    private static void Main(string[] args)
    {
        //args = [@"C:\Projects\SE\Directory.Build.props", "-AddProperty", "Demo", "-AddConstant", "DEMO"];

        if (args.Length == 0)
        {
            Console.WriteLine("first argument must be the .props file you want to edit" + usage);
            return;
        }
        string file = args[0];
        if (!file.EndsWith(".props"))
        {
            Console.WriteLine($"file must end with .props." + usage);
            return;
        }
        if (!File.Exists(file))
        {
            Console.WriteLine($"first argument must be the .props file you want to edit. The file {file} does not exist." + usage);
            return;
        }

        Dictionary<ActionType, List<string>> actions = GetActions(args.Skip(1).ToArray());

        string xml = File.ReadAllText(file);
        string xmlStart = xml;

        foreach (var removeProperty in actions[ActionType.RemoveProperty])
        {
            string template = $"<{removeProperty}>{{0}}</{removeProperty}>";
            xml = xml.Replace(string.Format(template, "true"), string.Format(template, "false"));
        }
        foreach (var addProperty in actions[ActionType.AddProperty])
        {
            string template = $"<{addProperty}>{{0}}</{addProperty}>";
            xml = xml.Replace(string.Format(template, "false"), string.Format(template, "true"));
        }

        if (actions[ActionType.AddConstant].Count > 0 || actions[ActionType.RemoveConstant].Count > 0)
        {
            const string DefineConstants = "<DefineConstants>";
            int startIndex = xml.IndexOf(DefineConstants);
            if (startIndex != -1)
            {
                startIndex += DefineConstants.Length;
                int endIndex = xml.IndexOf("<", startIndex);
                if (endIndex != -1)
                {
                    string constantsStr = xml.Substring(startIndex, endIndex - startIndex);
                    List<string> constants = constantsStr.Split([";"], StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var remove in actions[ActionType.RemoveConstant])
                    {
                        constants.Remove(remove);
                    }
                    // first remove to be added constants, to ensure they are not listed twice
                    foreach (var remove in actions[ActionType.AddConstant])
                    {
                        constants.Remove(remove);
                    }
                    foreach (var add in actions[ActionType.AddConstant])
                    {
                        constants.Add(add);
                    }
                    if (actions[ActionType.AddConstant].Count > 0)
                    {
                        constants.Sort();
                    }
                    xml = xml.Remove(startIndex) + string.Join(';', constants) + xml.Substring(endIndex);
                }
            }
        }

        if (xml != xmlStart)
        {
            File.WriteAllText(file, xml);
        }
    }

    private static Dictionary<ActionType, List<string>> GetActions(string[] args)
    {
        Dictionary<ActionType, List<string>> actions = new();
        foreach (var item in Enum.GetValues<ActionType>())
        {
            actions.Add(item, new());
        }

        ActionType currentAction = ActionType.None;
        foreach (var arg in args)
        {
            ActionType? newAction = GetNewAction(arg);

            if (newAction != null)
            {
                currentAction = newAction.Value;
            }
            else if (currentAction != ActionType.None)
            {
                actions[currentAction].Add(arg);
            }
        }

        return actions;

        ActionType? GetNewAction(string arg)
        {
            foreach (var item in actions)
            {
                if (arg == "-" + item.Key)
                {
                    return item.Key;
                }
            }
            return null;
        }
    }
}