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
        public KeyInput Keys { get; }
        public MouseInput Mouse { get; }
        public GamePadInput GamePad { get; }
        public KeyInput KeysDev { get; }
        public MouseInput MouseDev { get; }
        public GamePadInput GamePadDev { get; }
        public Random Rand { get; private set; } // is directly initialized with CreateInputRecorder
        public Int2 Resolution => inputSource.Current.WindowResolution;
        public Int2 GetResolution() => inputSource.Current.WindowResolution;
        private int randSeed;
        public int RecordingUpdate { get; private set; } = 0;

        public Action? OnPlayInput, OnPlayInputFinish;
        private IInputMetaObjectManager? metaObjectManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public InputStuff(bool mouseWithActivationClick, WindowManager windowManager, GameWrapper game, DefaultPaths basePaths, Action<Action> startRecordingPlayback, bool startRecordingInstantly, bool enableDevInput)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            stuff = new StuffDisposable(typeof(IUpdate));

            Func<MouseState> getMouseState;
            var mouseSource = new MouseWithoutOutOfWindowClicks(Microsoft.Xna.Framework.Input.Mouse.GetState, windowManager);

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
                Keyboard.GetState(), 
                Microsoft.Xna.Framework.Input.GamePad.GetState(0), 
                new MetaInputState(game.IsActivatedThisFrame()),
                windowManager.Resolution)));
            inputSource.OnUpdate += InputSource_OnUpdate;

            stuff.Add(Keys = new KeyInput(() => inputSource.Current.KeyState));

            if (enableDevInput)
            {
                KeysDev = new KeyInput(Keyboard.GetState);
                MouseDev = new MouseInput(Microsoft.Xna.Framework.Input.Mouse.GetState, game.IsActivatedThisFrame);
                GamePadDev = new GamePadInput(() => Microsoft.Xna.Framework.Input.GamePad.GetState(0));
            }
            else
            {
                KeysDev = new KeyInput(() => default);
                MouseDev = new MouseInput(() => default, () => false);
                GamePadDev = new GamePadInput(() => default);
            }

            stuff.Add(Mouse = new MouseInput(() => inputSource.Current.MouseState, () => inputSource.Current.MetaState.IsActivatedThisUpdate));
            stuff.Add(GamePad = new GamePadInput(() => inputSource.Current.GamePadState));

            stuff.Add(InputRecordingManager = new(stuff, inputSource, CreateInputRecorder, PlayInput));
            stuff.Add(InputRecordingTriggerer = new(KeysDev, InputRecordingManager, basePaths.InputRecordingsDir, startRecordingPlayback, startRecordingInstantly));
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

        public void UpdateDevInput()
        {
            KeysDev.Update();
            MouseDev.Update();
            GamePadDev.Update();
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
                Directory.CreateDirectory(dir);

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
                throw new BytingException("this input recording version is not supported: " + version);
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
