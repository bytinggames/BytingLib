using System.Diagnostics;

namespace BytingLib
{
    public class TargetGameSpeedTarget(Action<double?> onIntervalChange)
    {
        private double? interval;
        private double seconds;
        private readonly Stopwatch stopwatch = new();
        public double? MaxElapsedTime { get; set; }
        private TimeSpan intervalTimeSpan;

        public double? Interval
        {
            get => interval;
            set
            {
                if (value != interval)
                {
                    interval = value;
                    if (value != null)
                    {
                        intervalTimeSpan = TimeSpan.FromSeconds(value.Value);
                    }
                    seconds = 0;
                    onIntervalChange(value);
                }
            }
        }

        public float Extrapolation => interval == null ? 0f : (float)Math.Clamp(seconds / interval.Value, 0d, 1d);

        public bool ShouldSkip(TimeSpan monogameTargetElapsedTime)
        {
            if (Interval != null
                && monogameTargetElapsedTime != intervalTimeSpan) // if monogame already updates at the same interval, we don't need to filter updates
            {
                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
                else
                {
                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    elapsed = Math.Min(elapsed, MaxElapsedTime ?? (Interval.Value * 10));
                    seconds += elapsed;
                    stopwatch.Restart();
                    if (seconds >= Interval)
                    {
                        seconds -= Interval.Value;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
