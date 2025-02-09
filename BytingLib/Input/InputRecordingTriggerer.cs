namespace BytingLib
{
    public class InputRecordingTriggerer<T> : IUpdate where T : struct
    {
        private readonly InputInputRecordings? input;
        private readonly InputRecordingManager<T> inputRecordingManager;
        private readonly string inputRecordingDir;
        private readonly Action<Action> onStartPlaying;

        public InputRecordingTriggerer(InputInputRecordings? input, InputRecordingManager<T> inputRecordingManager, string inputRecordingDir, Action<Action> onStartPlaying, bool startRecordingInstantly)
        {
            this.input = input;
            this.inputRecordingManager = inputRecordingManager;
            this.inputRecordingDir = inputRecordingDir;
            this.onStartPlaying = onStartPlaying;

            if (inputRecordingDir != null && startRecordingInstantly)
            {
                StartRecording();
            }
        }

        public void Update()
        {
            if (input != null)
            {
                if (input.StartRecording.Pressed)
                {
                    Record();
                }
                else if (input.StopRecording.Pressed)
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
            Directory.CreateDirectory(inputRecordingDir);

            return Path.Combine(inputRecordingDir, DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss_fff") + ".inr");
        }

        private string? GetLastRecordingFile()
        {
            if (!Directory.Exists(inputRecordingDir))
            {
                return null;
            }
            string[] files = Directory.GetFiles(inputRecordingDir, "*.inr");
            if (files.Length == 0)
            {
                return null;
            }

            Array.Sort(files);
            return files[0];
        }

        public void StartRecording()
        {
            inputRecordingManager.StartRecording(GetNewRecordingFile());
        }
    }

}
