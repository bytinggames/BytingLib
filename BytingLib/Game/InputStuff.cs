using BytingLib.Serialization;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputStuff : IDisposable, IUpdate
    {
        protected readonly IStuffDisposable stuff;
        protected readonly StructSource<FullInput> inputSource;

        public InputRecordingManager<FullInput> InputRecordingManager { get; }
        public InputRecordingTriggerer<FullInput> InputRecordingTriggerer { get; }
        public MouseInput Mouse { get; }
        public GamePadInput GamePad { get; }
        public Random Rand { get; private set; } = new Random(); // is initialized again with CreateInputRecorder. This random initialization is just a fallback
        public Int2 Resolution => inputSource.Current.WindowResolution;
        public Int2 GetResolution() => inputSource.Current.WindowResolution;
        private int randSeed;
        public int RecordingUpdate { get; private set; } = 0;

        public MouseState CurrentMouseState { get; private set; }
        public KeyboardState CurrentKeyState { get; private set; }

        public Action? OnPlayInput, OnPlayInputFinish;
        private IInputMetaObjectManager? metaObjectManager;

        public GamePadDeadZone GamePadDeadZoneLeft { get; set; } = GamePadDeadZone.IndependentAxes;
        public GamePadDeadZone GamePadDeadZoneRight { get; set; } = GamePadDeadZone.IndependentAxes;
        public FullInput FullInput => inputSource.Current;

        public InputStuff(bool mouseWithActivationClick, WindowManager windowManager, GameWrapper game, DefaultPaths basePaths,
            Action<Action> startRecordingPlayback, bool startRecordingInstantly, InputRecordingBinds? inputRecordingBinds)
        {
            CurrentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            CurrentKeyState = Keyboard.GetState();
            void SetMousePosition(Vector2 pos) => Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)MathF.Round(pos.X), (int)MathF.Round(pos.Y));

            stuff = new StuffDisposable(typeof(IUpdate));

            Func<MouseState> getMouseState;
            var mouseSource = new MouseWithoutOutOfWindowClicks(() => CurrentMouseState, windowManager);

            if (mouseWithActivationClick)
            {
                getMouseState = mouseSource.GetState;
            }
            else
            {
                MouseWithoutActivationClicks mouseFiltered = new MouseWithoutActivationClicks(mouseSource.GetState, f => game.Activated += f);
                getMouseState = mouseFiltered.GetState;
            }

            stuff.Add(inputSource = new StructSource<FullInput>(() =>
                new FullInput(getMouseState(),
                CurrentKeyState,
                Microsoft.Xna.Framework.Input.GamePad.GetState(0),
                new MetaInputState(game.IsActivatedThisFrame()),
                windowManager.Resolution)));
            inputSource.OnUpdate += InputSource_OnUpdate;

            stuff.Add(Mouse = new MouseInput(() => inputSource.Current.MouseState, () => inputSource.Current.MetaState.IsActivatedThisUpdate, SetMousePosition));
            stuff.Add(GamePad = new GamePadInput(() => inputSource.Current.GamePadState));

            stuff.Add(InputRecordingManager = new(stuff, inputSource, CreateInputRecorder, PlayInput));
            stuff.Add(InputRecordingTriggerer = new(inputRecordingBinds, InputRecordingManager, basePaths.InputRecordingsDir, startRecordingPlayback, startRecordingInstantly));
        }

        private void InputSource_OnUpdate(FullInput obj)
        {
            RecordingUpdate++;
        }

        public void SetMetaObjectManager(IInputMetaObjectManager? metaObjectManager)
        {
            this.metaObjectManager = metaObjectManager;
        }

        public void Update()
        {
            stuff.ForEach<IUpdate>(f => f.Update());
        }

        public void PreUpdate()
        {
            CurrentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            CurrentKeyState = Keyboard.GetState();
        }

        public void Dispose()
        {
            stuff.Dispose();
        }

        private IDisposable CreateInputRecorder(StructSource<FullInput> inputSource, string path)
        {
            string dir = Path.GetDirectoryName(path)!;
            // make sure directory exists
            if (dir != "" // in this case, the directory is the current directory and it is already existing
                && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            FileStream fs = File.Create(path);
            BinaryWriter writer = new(fs);

            WriteVersion(writer);
            WriteSeed(writer);
            WriteMetaObject(writer);
            BeginWritingInputStream(fs);

            StructStreamWriterCompressed<FullInput> recorder = new(fs, true);
            inputSource.OnUpdate += AddState;

            return new OnDispose(() =>
            {
                inputSource.OnUpdate -= AddState;
                recorder.Dispose();
                writer.Dispose();
                fs.Dispose();
            });

            void AddState(FullInput state) => recorder.AddState(state);
        }

        private void BeginWritingInputStream(FileStream fs)
        {
            fs.WriteByte((byte)MetaData.InputStream);
        }

        private void PlayInput(StructSource<FullInput> inputSource, string path, Action onFinish)
        {
            RecordingUpdate = 0;

            OnPlayInput?.Invoke();

            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new(fs);

            MetaData metaData;
            while ((metaData = (MetaData)fs.ReadByte()) != MetaData.InputStream)
            {
                switch (metaData)
                {
                    case MetaData.Version:
                        ReadVersion(reader);
                        break;
                    case MetaData.Seed:
                        ReadSeed(reader);
                        break;
                    case MetaData.Object:
                        ReadMetaObject(reader);
                        break;
                    default:
                        throw new BytingException("couldn't read meta data of input file");
                }
            }

            StructStreamReaderCompressed<FullInput> playback = new(fs);
            inputSource.SetSource(playback, _ =>
            {
                metaObjectManager?.OnReplayEnd();
                playback.Dispose();
                reader.Dispose();
                fs.Dispose();

                onFinish?.Invoke();

                OnPlayInputFinish?.Invoke();
            });
        }

        private void WriteVersion(BinaryWriter writer)
        {
            writer.Write((byte)MetaData.Version);
            writer.Write(1); // version
        }

        private void ReadVersion(BinaryReader reader)
        {
            int version = reader.ReadInt32(); // read version
            if (version != 1)
            {
                throw new BytingException("this input recording version is not supported: " + version);
            }
        }

        private void WriteSeed(BinaryWriter writer)
        {
            writer.Write((byte)MetaData.Seed);

            randSeed = new Random().Next();
            Rand = new Random(randSeed);

            writer.Write(randSeed);
        }
        private void ReadSeed(BinaryReader reader)
        {
            int seed = reader.ReadInt32();
            Rand = new Random(seed);
        }

        private void WriteMetaObject(BinaryWriter writer)
        {
            if (metaObjectManager != null)
            {
                writer.Write((byte)MetaData.Object);
                metaObjectManager.WriteMetaObject(writer);
            }
        }
        private void ReadMetaObject(BinaryReader reader)
        {
            if (metaObjectManager != null)
            {
                metaObjectManager.ReadMetaObject(reader);
            }
        }

        enum MetaData
        {
            InputStream = 0,
            Seed = 1,
            Object = 2,
            Version = 3,
        }
    }
}
