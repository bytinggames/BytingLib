using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public class TargetGameSpeedTarget(Action<double?> onIntervalChange)
    {
        private double? intervalSeconds;
        private double seconds;
        private double lastUpdateSecondsTimestamp;
        private readonly Stopwatch stopwatch = new();
        public double? MaxElapsedTime { get; set; }
        private TimeSpan intervalTimeSpan;
        public GameTime GameTime { get; } = new();

        public double? IntervalSeconds
        {
            get => intervalSeconds;
            set
            {
                if (value != intervalSeconds)
                {
                    intervalSeconds = value;
                    if (value != null)
                    {
                        intervalTimeSpan = TimeSpan.FromSeconds(value.Value);
                    }
                    seconds = 0;
                    onIntervalChange(value);
                }
            }
        }

        public float Extrapolation => intervalSeconds == null ? 0f : (float)Math.Clamp((seconds - lastUpdateSecondsTimestamp) / intervalSeconds.Value, 0d, 1d);

        public bool ShouldSkip(TimeSpan monogameTargetElapsedTime)
        {
            if (IsMonoGameResponsible(monogameTargetElapsedTime)) // if monogame already updates at the same interval, we don't need to filter updates
            {
                GameTime.TotalGameTime += monogameTargetElapsedTime;
                GameTime.ElapsedGameTime = monogameTargetElapsedTime;
            }
            else
            {
                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                    GameTime.TotalGameTime += intervalTimeSpan;
                    GameTime.ElapsedGameTime = intervalTimeSpan;
                }
                else
                {
                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();

                    elapsed = Math.Min(elapsed, MaxElapsedTime ?? (IntervalSeconds.Value * 10));
                    seconds += elapsed;
                    if (seconds >= IntervalSeconds)
                    {
                        seconds -= IntervalSeconds.Value;
                        lastUpdateSecondsTimestamp = seconds;
                        GameTime.TotalGameTime += intervalTimeSpan;
                        GameTime.ElapsedGameTime = intervalTimeSpan;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [MemberNotNullWhen(false, nameof(IntervalSeconds))]
        private bool IsMonoGameResponsible(TimeSpan monogameTargetElapsedTime)
        {
            return IntervalSeconds == null
                || monogameTargetElapsedTime == intervalTimeSpan;
        }
    }
}
