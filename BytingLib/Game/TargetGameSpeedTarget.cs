using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public class TargetGameSpeedTarget(Action<double?> onIntervalChange, bool realOrFixedTime, bool unlimitedTicksOnDisabledTimeStep)
    {
        private double? intervalSeconds;
        private double seconds;
        private double lastUpdateSecondsTimestamp;
        private readonly Stopwatch stopwatch = new();
        private TimeSpan intervalTimeSpan;
        private readonly bool realOrFixedTime = realOrFixedTime;
        private Stopwatch stopwatchRealtime = new();
        private readonly bool unlimitedTicksOnDisabledTimeStep = unlimitedTicksOnDisabledTimeStep;

        public double? MaxElapsedTime { get; set; }
        /// <summary>Only used if MaxElapsedTime = null. Then max elapsed time is set to IntervalSeconds * MaxElapsedTimeFactor</summary>
        public double MaxElapsedTimeFactor { get; set; } = 3d;
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

        /// <summary>If fixedTimeStep is set to false, the target elapsed time is ignored by monogame (like it should be)</summary>
        public bool ShouldSkip(TimeSpan monogameTargetElapsedTime, bool fixedTimeStep)
        {
            if (ShouldSkipInner(monogameTargetElapsedTime, fixedTimeStep))
            {
                return true;
            }
            else
            {
                if (realOrFixedTime)
                {
                    // real time
                    if (stopwatchRealtime.IsRunning)
                    {
                        TimeSpan elapsed = stopwatchRealtime.Elapsed;

                        double maxElapsed = GetMaxElapsedTime(IntervalSeconds ?? monogameTargetElapsedTime.TotalSeconds);
                        if (elapsed.TotalSeconds > maxElapsed)
                        {
                            elapsed = TimeSpan.FromSeconds(maxElapsed);
                        }

                        stopwatchRealtime.Restart();
                        GameTime.ElapsedGameTime = elapsed;
                    }
                    else
                    {
                        stopwatchRealtime.Start();
                        // if time isn't measured yet, use fixed time
                        SetElapsedToFixedTime(monogameTargetElapsedTime);
                    }
                }
                else
                {
                    // fixed time
                    SetElapsedToFixedTime(monogameTargetElapsedTime);
                }
                GameTime.TotalGameTime += GameTime.ElapsedGameTime;

                return false;
            }

        }

        private void SetElapsedToFixedTime(TimeSpan monogameTargetElapsedTime)
        {
            if (IntervalSeconds == null)
            {
                GameTime.ElapsedGameTime = monogameTargetElapsedTime;
            }
            else
            {
                GameTime.ElapsedGameTime = TimeSpan.FromSeconds(IntervalSeconds.Value);
            }
        }

        private bool ShouldSkipInner(TimeSpan monogameTargetElapsedTime, bool fixedTimeStep)
        {
            if (!IsMonoGameResponsible(monogameTargetElapsedTime, fixedTimeStep)) // if monogame already updates at the same interval, we don't need to filter updates
            {
                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
                else
                {
                    double elapsed = stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Restart();

                    seconds += elapsed;

                    // update up to 10 updates
                    var maxSeconds = GetMaxElapsedTime(IntervalSeconds.Value);
                    if (seconds >= maxSeconds)
                    {
                        seconds = maxSeconds;
                    }

                    if (seconds >= IntervalSeconds)
                    {
                        seconds -= IntervalSeconds.Value;
                        lastUpdateSecondsTimestamp = seconds;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private double GetMaxElapsedTime(double intervalSeconds)
        {
            return MaxElapsedTime ?? intervalSeconds * MaxElapsedTimeFactor;
        }

        [MemberNotNullWhen(false, nameof(IntervalSeconds))]
        private bool IsMonoGameResponsible(TimeSpan monogameTargetElapsedTime, bool fixedTimeStep)
        {
            return IntervalSeconds == null
                || (monogameTargetElapsedTime == intervalTimeSpan && fixedTimeStep)
                || (!fixedTimeStep && unlimitedTicksOnDisabledTimeStep);
        }
    }
}
