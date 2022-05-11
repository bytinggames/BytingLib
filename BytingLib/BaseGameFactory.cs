using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public static class BaseGameFactory
    {
        public static IStuffDisposable CreateDefaultGame(Game game, GraphicsDeviceManager graphics, out KeyInput keys, out MouseInput mouse, out GamePadInput gamePad, bool mouseWithActivationClick = true)
        {
            IStuffDisposable stuff = new StuffDisposable(typeof(IUpdate), typeof(IDraw));

            KeyInput keysDev;
            StructSource<FullInput> inputSource;
            InputRecordingManager<FullInput> inputRecordingManager;

            Func<MouseState> getMouseState;

            if (mouseWithActivationClick)
                getMouseState = Mouse.GetState;
            else
            {
                MouseWithoutActivationClick mouseFiltered = new MouseWithoutActivationClick(Mouse.GetState, f => game.Activated += f);
                getMouseState = mouseFiltered.GetState;
            }

            stuff.Add(inputSource = new StructSource<FullInput>(() => new FullInput(getMouseState(), Keyboard.GetState(), GamePad.GetState(0))));
            KeyInput _keys = new KeyInput(() => inputSource.Current.KeyState);
            stuff.Add(keys = _keys);
#if DEBUG
            stuff.Add(keysDev = new KeyInput(Keyboard.GetState));
#else
            stuff.Add(keysDev = new KeyInput(() => default));
#endif
            stuff.Add(mouse = new MouseInput(() => inputSource.Current.MouseState));
            stuff.Add(gamePad = new GamePadInput(() => inputSource.Current.GamePadState));
            stuff.Add(inputRecordingManager = new InputRecordingManager<FullInput>(stuff, inputSource, CreateInputRecorder, PlayInput));

            stuff.Add(new UpdateKeyPressed(keys, Keys.Escape, game.Exit));
            stuff.Add(new InputRecorderTrigger<FullInput>(keysDev, inputRecordingManager));

            WindowManager windowManager = new WindowManager(true, game.Window, graphics);
            game.Window.AllowUserResizing = true;
            stuff.Add(new UpdateKeyPressed(keys, Keys.F11, windowManager.ToggleFullscreen));
            stuff.Add(new UpdateKeyPressed(keysDev, Keys.Right, windowManager.SwapScreen));
            stuff.Add(new UpdateKeyPressed(keysDev, Keys.Left, windowManager.SwapScreen));

            return stuff;
        }

        class InputRecorderTrigger<T> : IUpdate where T : struct
        {
            private readonly KeyInput keys;
            private readonly InputRecordingManager<T> inputRecordingManager;

            public InputRecorderTrigger(KeyInput keys, InputRecordingManager<T> inputRecordingManager)
            {
                this.keys = keys;
                this.inputRecordingManager = inputRecordingManager;
            }
            public void Update()
            {
                if (keys.F5.Pressed)
                {
                    if (keys.Control.Down)
                        inputRecordingManager.ToggleRecording("input.txt");
                    else
                        inputRecordingManager.TogglePlaying("input.txt");
                }
            }
        }

        private static IDisposable CreateInputRecorder(StructSource<FullInput> inputSource, string path)
        {
            FileStream fs = File.Create(path);
            StructStreamWriterCompressed<FullInput> recorder = new(fs);
            inputSource.OnUpdate += AddState;

            return new OnDispose(() =>
            {
                inputSource.OnUpdate -= AddState;
                recorder.Dispose();
                fs.Dispose();
            });

            void AddState(FullInput state) => recorder.AddState(state);
        }

        private static void PlayInput(StructSource<FullInput> inputSource, string path, Action onFinish)
        {
            FileStream fs = File.OpenRead(path);
            StructStreamReaderCompressed<FullInput> playback = new(fs);
            inputSource.SetSource(playback, _ =>
            {
                playback.Dispose();
                fs.Dispose();

                onFinish?.Invoke();
            });
        }
    }
}
