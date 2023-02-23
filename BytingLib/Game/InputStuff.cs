using BytingLib.Serialization;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class InputStuff : IDisposable, IUpdate
    {
        protected readonly IStuffDisposable stuff;
        protected readonly InputRecordingManager<FullInput> inputRecordingManager;
        protected readonly InputRecordingTriggerer<FullInput> inputRecordingTriggerer;
        protected readonly StructSource<FullInput> inputSource;

        public KeyInput Keys { get; }
        public MouseInput Mouse { get; }
        public GamePadInput GamePad { get; }
        public KeyInput KeysDev { get; }
        public Random Rand { get; private set; } // is directly initialized with CreateInputRecorder
        public Int2 Resolution => inputSource.Current.WindowResolution;
        public Int2 GetResolution() => inputSource.Current.WindowResolution;
        private int randSeed;

        public Action? OnPlayInput;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public InputStuff(bool mouseWithActivationClick, WindowManager windowManager, GameWrapper game, DefaultPaths basePaths)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            stuff = new StuffDisposable(typeof(IUpdate));

            Func<MouseState> getMouseState;
            var mouseSource = new MouseWithoutOutOfWindowClicks(Microsoft.Xna.Framework.Input.Mouse.GetState, windowManager);

            if (mouseWithActivationClick)
                getMouseState = mouseSource.GetState;
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

            stuff.Add(Keys = new KeyInput(() => inputSource.Current.KeyState));
#if DEBUG
            KeysDev = new KeyInput(Keyboard.GetState);
#else
            KeysDev = new KeyInput(() => default);
#endif
            stuff.Add(Mouse = new MouseInput(() => inputSource.Current.MouseState, () => inputSource.Current.MetaState.IsActivatedThisUpdate));
            stuff.Add(GamePad = new GamePadInput(() => inputSource.Current.GamePadState));

            stuff.Add(inputRecordingManager = new(stuff, inputSource, CreateInputRecorder, PlayInput));
            stuff.Add(inputRecordingTriggerer = new(KeysDev, inputRecordingManager, basePaths.InputRecordingsDir));
        }

        public void Update()
        {
            stuff.ForEach<IUpdate>(f => f.Update());
        }

        public void UpdateKeysDev()
        {
            KeysDev.Update();
        }

        public void Dispose()
        {
            stuff.Dispose();
        }

        private IDisposable CreateInputRecorder(StructSource<FullInput> inputSource, string path)
        {
            string dir = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileStream fs = File.Create(path);
            WriteSeed(fs);
            BeginWritingInputStream(fs);

            StructStreamWriterCompressed<FullInput> recorder = new(fs, true);
            inputSource.OnUpdate += AddState;

            return new OnDispose(() =>
            {
                inputSource.OnUpdate -= AddState;
                recorder.Dispose();
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
            OnPlayInput?.Invoke();

            FileStream fs = File.OpenRead(path);

            MetaData metaData;
            while ((metaData = (MetaData)fs.ReadByte()) != MetaData.InputStream)
            {
                switch (metaData)
                {
                    case MetaData.Seed:
                        ReadSeed(fs);
                        break;
                    default:
                        throw new BytingException("couldn't read meta data of input file");
                }
            }


            StructStreamReaderCompressed<FullInput> playback = new(fs);
            inputSource.SetSource(playback, _ =>
            {
                playback.Dispose();
                fs.Dispose();

                onFinish?.Invoke();
            });
        }

        private void WriteSeed(FileStream fs)
        {
            fs.WriteByte((byte)MetaData.Seed);

            randSeed = new Random().Next();
            Rand = new Random(randSeed);

            byte[] seedBytes = BitConverter.GetBytes(randSeed);
            fs.Write(seedBytes, 0, seedBytes.Length);
        }
        private void ReadSeed(FileStream fs)
        {
            byte[] seedBytes = new byte[4];
            fs.Read(seedBytes, 0, seedBytes.Length);
            int seed = BitConverter.ToInt32(seedBytes);
            Rand = new Random(seed);
        }

        enum MetaData
        {
            InputStream = 0,
            Seed = 1,
        }
    }
}
