using System.Diagnostics;

namespace BytingLib
{
    public class MeasureDurationItem
    {
        const int maxSamples = 60;

        List<long> samples = new();
        long totalTicks;
        Stopwatch sw = new();

        public string StackStr { get; }

        public MeasureDurationItem(int stack)
        {
            StackStr = "";
            for (int i = 0; i < stack; i++)
            {
                StackStr += "  ";
            }
        }

        internal void MeasureBegin()
        {
            sw.Restart();
        }

        internal void MeasureEnd()
        {
            sw.Stop();
            long ms = sw.ElapsedTicks;
            samples.Add(ms);
            totalTicks += ms;
            if (samples.Count > maxSamples)
            {
                totalTicks -= samples[0];
                samples.RemoveAt(0);
            }
        }

        public float GetAverageMS()
        {
            if (samples.Count == 0)
            {
                return -1;
            }

            return (float)totalTicks / TimeSpan.TicksPerMillisecond / samples.Count;
        }
    }
}