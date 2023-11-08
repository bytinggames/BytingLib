
namespace BytingLib
{
    public class InputRecordingManager<T> : IDisposable where T : struct
    {
        private readonly IStuffDisposable parent;
        private readonly StructSource<T> inputSource;
        private readonly CreateInputRecorder createInputRecorder;
        private readonly PlayInput playInput;

        private IDisposable? _recorder;
        private bool _playing = false;

        /// <summary>Only used for triggering OnStateChanged event.</summary>
        private InputRecordingState currentStateStored;
        public event Action<InputRecordingState>? OnStateChanged;

        public InputRecordingManager(IStuffDisposable parent, StructSource<T> inputSource, CreateInputRecorder createInputRecorder, PlayInput playInput)
        {
            this.parent = parent;
            this.inputSource = inputSource;
            this.createInputRecorder = createInputRecorder;
            this.playInput = playInput;

            currentStateStored = CurrentState;
        }

        public delegate IDisposable CreateInputRecorder(StructSource<T> inputSource, string filePath);
        public delegate void PlayInput(StructSource<T> inputSource, string path, Action onFinish);

        private bool Playing
        {
            get => _playing;
            set
            {
                _playing = value;
                if (currentStateStored != CurrentState)
                {
                    currentStateStored = CurrentState;
                    OnStateChanged?.Invoke(currentStateStored);
                }
            }
        }
        private IDisposable? Recorder
        {
            get => _recorder;
            set
            {
                _recorder = value;
                if (currentStateStored != CurrentState)
                {
                    currentStateStored = CurrentState;
                    OnStateChanged?.Invoke(currentStateStored);
                }
            }
        }
        public InputRecordingState CurrentState
        {
            get
            {
                if (Playing)
                {
                    return InputRecordingState.Playing;
                }
                else if (Recorder != null)
                {
                    return InputRecordingState.Recording;
                }
                else
                {
                    return InputRecordingState.None;
                }
            }
        }

        public void ToggleRecording(string filePath)
        {
            if (CurrentState == InputRecordingState.Recording)
            {
                StopRecording();
            }
            else
            {
                StartRecording(filePath);
            }
        }

        public void StartRecording(string filePath)
        {
            if (Playing)
            {
                StopPlaying();
            }

            if (Recorder != null)
            {
                StopRecording();
            }

            parent.Add(Recorder = createInputRecorder(inputSource, filePath));
        }

        public void TogglePlaying(string filePath)
        {
            if (CurrentState == InputRecordingState.Playing)
            {
                StopPlaying();
            }
            else
            {
                StartPlaying(filePath);
            }
        }

        public void StartPlaying(string filePath)
        {
            if (Recorder != null)
            {
                StopRecording();
            }

            if (Playing)
            {
                StopPlaying();
            }

            if (File.Exists(filePath))
            {
                Playing = true;
                playInput(inputSource, filePath, (Action)(() =>
                {
                    this.Playing = false;
                }));
            }
        }

        public void StopPlaying()
        {
            inputSource.RemoveSource();
        }

        public void StopRecording()
        {
            if (Recorder != null)
            {
                parent.Remove(Recorder);
                Recorder = null;
            }
        }

        public void Dispose()
        {
            StopPlaying();
            StopRecording();
        }
    }
}
