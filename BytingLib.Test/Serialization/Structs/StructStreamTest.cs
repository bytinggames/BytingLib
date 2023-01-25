using BytingLib.Test.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BytingLib.Test.Serialization.Structs
{
    [TestClass]
    public class StructStreamTest
    {
        static IEnumerable<object[]> Frames => new[]
        {
            new object[]{ 30 },
            new object[]{ 0 },
            new object[]{ 1 },
            new object[]{ 2 },
            new object[]{ 100 },
            new object[]{ 60 * 60 * 60 } // 1h of gameplay
        };

        [TestMethod]
        public void TestEmptyPlayback()
        {
            IEnumerator<KeyboardState> playback;
            using (MemoryStream ms = new MemoryStream())
            {
                playback = new StructStreamReaderCompressed<KeyboardState>(ms);
            }
            var list = PlayBackKeys(playback).ToList();
            Assert.AreEqual(0, list.Count);
        }

        public void TestRecording<TRecorder, TPlayback>(int forFrames)
            where TRecorder : IStructStreamWriter<KeyboardState>
            where TPlayback : IEnumerator<KeyboardState>
        {
            List<KeyboardState> keyStatesPerFrame, playBackKeys;

            using (MemoryStream ms = new MemoryStream())
            {
                using (var recorder = (TRecorder)Activator.CreateInstance(typeof(TRecorder), ms, false)!)
                    keyStatesPerFrame = RecordKeys(recorder, forFrames);

                using (var playback = (TPlayback)Activator.CreateInstance(typeof(TPlayback), ms, 0)!)
                    playBackKeys = PlayBackKeys(playback).ToList();


                Console.WriteLine("size of recording: " + ms.Length + " bytes");
            }
            Assert.AreEqual(keyStatesPerFrame.Count, playBackKeys.Count);
            for (int i = 0; i < keyStatesPerFrame.Count; i++)
            {
                Assert.AreEqual(keyStatesPerFrame[i], playBackKeys[i], "difference in frame " + i);
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Frames))]
        public void TestRecordingCompressed(int forFrames)
        {
            TestRecording<StructStreamWriterCompressed<KeyboardState>, StructStreamReaderCompressed<KeyboardState>>(forFrames);
        }

        private List<KeyboardState> RecordKeys(IStructStreamWriter<KeyboardState> recorder, int forFrames)
        {
            using IEnumerator<KeyboardState> keyStateEnumerator = GetKeyState().GetEnumerator();

            List<KeyboardState> keyStatesPerFrame = new List<KeyboardState>();

            KeyInput keys = new KeyInput(() =>
            {
                keyStateEnumerator.MoveNext();
                return keyStateEnumerator.Current;
            });
            for (int i = 0; i < forFrames; i++)
            {
                keys.Update();
                recorder.AddState(keys.GetState());
                keyStatesPerFrame.Add(keys.GetState());
            }

            return keyStatesPerFrame;
        }

        private IEnumerable<KeyboardState> PlayBackKeys(IEnumerator<KeyboardState> playback)
        {
            while (playback.MoveNext())
            {
                yield return playback.Current;
            }
        }

        IEnumerable<KeyboardState> GetKeyState()
        {
            while (true)
            {
                KeyboardState ks = default;
                yield return ks;
                InputSimulation.SimulateSpacePress(ref ks);
                yield return ks;
                yield return ks;
                yield return ks;
                ks = default;
                yield return ks;
                yield return ks;
                InputSimulation.SimulatePress(ref ks, 3, 1);
                yield return ks;
                yield return ks;
                InputSimulation.SimulatePress(ref ks, 4, 1);
                yield return ks;
                InputSimulation.SimulatePress(ref ks, 3, 0);
                yield return ks;
                InputSimulation.SimulatePress(ref ks, 4, 0);
                yield return ks;
                yield return ks;
                InputSimulation.SimulatePress(ref ks, 4, 1);
                InputSimulation.SimulatePress(ref ks, 3, 1);
                yield return ks;
                ks = default;
                // silence for a bit
                for (int i = 0; i < byte.MaxValue * 2; i++)
                    yield return ks;
            }
        }
    }
}
