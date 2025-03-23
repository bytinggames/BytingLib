namespace BytingLib
{
    public class TargetGameSpeed
    {
        public TargetGameSpeedTarget Update { get; }
        public TargetGameSpeedTarget Draw { get; }
        public Action<double>? SetTargetElapsedSeconds;

        public TargetGameSpeed(double? updateInterval)
        {
            Update = new TargetGameSpeedTarget(_ => UpdateGameTickInterval(), false);
            Draw = new TargetGameSpeedTarget(_ => UpdateGameTickInterval(), true);
            Update.IntervalSeconds = updateInterval;
        }

        private void UpdateGameTickInterval()
        {
            if (SetTargetElapsedSeconds == null)
            {
                return;
            }
            double targetElapsed;
            if (Draw.IntervalSeconds == null && Update.IntervalSeconds == null)
            {
                // default, when Draw.Interval and Update.Interval are unset
                targetElapsed = 1d / 60d;
            }
            else
            {
                targetElapsed = double.MaxValue;
                if (Draw.IntervalSeconds != null)
                {
                    targetElapsed = Draw.IntervalSeconds.Value;
                }
                if (Update.IntervalSeconds != null)
                {
                    targetElapsed = Math.Min(Update.IntervalSeconds.Value, targetElapsed);
                }
                if (targetElapsed <= 0)
                {
                    targetElapsed = 1d / 60d;
                }
            }
            SetTargetElapsedSeconds.Invoke(targetElapsed);
        }
    }
}
