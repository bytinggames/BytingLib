namespace BytingLib
{
    public class SoundSettings
    {
        public class SoundSetting
        {
            public float Volume = 0.5f;
            public float Pitch = 0f;

            internal void ApplyTo(SoundItem sound)
            {
                sound.Volume = Volume;
                sound.Pitch = Pitch;
            }
        }

        public Dictionary<string, SoundSetting> Settings { get; } = new Dictionary<string, SoundSetting>();

        public SoundSettings(string soundSettingsTxt)
        {
            string[] lines = soundSettingsTxt.Replace("\r", "").Split(new char[] { '\n' });

            List<string> folderStack = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("//"))
                {
                    continue;
                }

                int tabs = 0;

                while (lines[i].Length > tabs && lines[i][tabs] == '\t')
                {
                    tabs++;
                }
                if (tabs > 0)
                {
                    lines[i] = lines[i].Substring(tabs);
                }

                while (folderStack.Count > tabs)
                {
                    folderStack.RemoveAt(folderStack.Count - 1);
                }

                string[] split = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                folderStack.Add(split[0]);

                if (split.Length > 1)
                {
                    SoundSetting s = new SoundSetting();
                    for (int j = 1; j < split.Length; j++)
                    {
                        char c = split[j][0];
                        string rest = split[j].Substring(1);
                        float f;
                        switch (c)
                        {
                            case 'p':
                                if (float.TryParse(rest, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out f))
                                {
                                    s.Pitch = f;
                                }
                                break;
                            case 'v':
                                if (float.TryParse(rest, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out f))
                                {
                                    s.Volume = f;
                                }
                                break;
                        }
                    }

                    string fileName = "Sounds/" + string.Join('/', folderStack);

                    Settings.Add(fileName, s);
                }
            }
        }
    }
}
