
namespace BytingLib
{
    public class InputRecordingManager<T> : IDisposable where T : struct
    {
        private readonly IStuff parent;
        private readonly StructSource<T> inputSource;
        private readonly CreateInputRecorder createInputRecorder;
        private readonly PlayInput playInput;

        private IDisposable? recorder;
        private bool playing = false;


        public InputRecordingManager(IStuff parent, StructSource<T> inputSource, CreateInputRecorder createInputRecorder, PlayInput playInput)
        {
            this.parent = parent;
            this.inputSource = inputSource;
            this.createInputRecorder = createInputRecorder;
            this.playInput = playInput;
        }

        public delegate IDisposable CreateInputRecorder(StructSource<T> inputSource, string filePath);
        public delegate void PlayInput(StructSource<T> inputSource, string path, Action onFinish);

        public enum State
        {
            None,
            Playing,
            Recording
        }

        public State CurrentState
        {
            get
            {
                if (playing)
                    return State.Playing;
                else if (recorder != null)
                    return State.Recording;
                else
                    return State.None;
            }
        }

        public void ToggleRecording(string filePath)
        {
            if (CurrentState == State.Recording)
                StopRecording();
            else
                StartRecording(filePath);
        }

        public void StartRecording(string filePath)
        {
            if (playing)
                StopPlaying();

            if (recorder != null)
                StopRecording();

            parent.Add(recorder = createInputRecorder(inputSource, filePath));
        }

        public void TogglePlaying(string filePath)
        {
            if (CurrentState == State.Playing)
                StopPlaying();
            else
                StartPlaying(filePath);
        }

        public void StartPlaying(string filePath)
        {
            if (recorder != null)
                StopRecording();

            if (playing)
                StopPlaying();

            if (File.Exists(filePath))
            {
                playing = true;
                playInput(inputSource, filePath, () =>
                {
                    playing = false;
                });
            }
        }

        public void StopPlaying()
        {
            inputSource.RemoveSource();
        }

        public void StopRecording()
        {
            if (recorder != null)
            {
                parent.Remove(recorder);
                recorder = null;
            }
        }

        public void Dispose()
        {
            StopPlaying();
            StopRecording();
        }
    }
}
