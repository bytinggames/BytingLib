using System.Diagnostics;

namespace BytingLib
{
    public class FPS
    {
        public List<long> timestamps = new List<long>();

        Stopwatch sw;

        public void NewFrame()
        {
            if (sw == null)
            {
                sw = new Stopwatch();
                sw.Start();
            }

            timestamps.Add(sw.ElapsedMilliseconds);
            while (timestamps.Count > 0 && sw.ElapsedMilliseconds - timestamps[0] > 1000)
            {
                timestamps.RemoveAt(0);
            }
        }

        public void Pause()
        {
            if (sw != null)
                sw.Stop();
        }

        public void Continue()
        {
            if (sw != null)
                sw.Start();
        }

        public int CurrentFPS => sw.Elapsed.TotalSeconds == 0 ? -1 : (int)(timestamps.Count / Math.Min(1d, sw.Elapsed.TotalSeconds));
    }
}
