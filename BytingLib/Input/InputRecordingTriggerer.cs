namespace BytingLib
{
    public class InputRecordingTriggerer<T> : IUpdate where T : struct
    {
        private readonly KeyInput keys;
        private readonly InputRecordingManager<T> inputRecordingManager;
        private readonly string inputRecordingDir;
        private readonly Action<Action> onStartPlaying;
        private readonly bool controlViaF5;

        public InputRecordingTriggerer(KeyInput devKeys, InputRecordingManager<T> inputRecordingManager, string inputRecordingDir, Action<Action> onStartPlaying, bool startRecordingInstantly, bool controlViaF5 = true)
        {
            keys = devKeys;
            this.inputRecordingManager = inputRecordingManager;
            this.inputRecordingDir = inputRecordingDir;
            this.onStartPlaying = onStartPlaying;
            this.controlViaF5 = controlViaF5;

            if (inputRecordingDir != null && startRecordingInstantly)
            {
                StartRecording();
            }
        }

        public void Update()
        {
            if (controlViaF5 && keys.F5.Pressed)
            {
                if (keys.Shift.Down)
                {
                    Record();
                }
                else
                {
                    Play();
                }
            }
        }

        public void Play()
        {
            string? file = GetLastRecordingFile();
            if (file != null)
            {
                onStartPlaying(() =>
                {
                    inputRecordingManager.TogglePlaying(file);
                });
            }
        }

        public void Record()
        {
            inputRecordingManager.ToggleRecording(GetNewRecordingFile());
        }

        private string GetNewRecordingFile()
        {
            return Path.Combine(inputRecordingDir, DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss_fff") + ".inr");
        }

        private string? GetLastRecordingFile()
        {
            string[] files = Directory.GetFiles(inputRecordingDir, "*.inr");
            if (files.Length == 0)
                return null;

            Array.Sort(files);
            return files[0];
        }

        public void StartRecording()
        {
            inputRecordingManager.StartRecording(GetNewRecordingFile());
        }
    }

}
