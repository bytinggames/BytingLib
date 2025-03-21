namespace BytingLib
{
    public class TargetGameSpeed
    {
        public TargetGameSpeedTarget Update { get; }
        public TargetGameSpeedTarget Draw { get; }
        public Action<double>? SetTargetElapsedSeconds;

        public TargetGameSpeed(double? updateInterval)
        {
            Update = new TargetGameSpeedTarget(_ => UpdateGameTickInterval());
            Draw = new TargetGameSpeedTarget(_ => UpdateGameTickInterval());
            Update.Interval = updateInterval;
        }

        private void UpdateGameTickInterval()
        {
            if (SetTargetElapsedSeconds == null)
            {
                return;
            }
            double targetElapsed;
            if (Draw.Interval == null && Update.Interval == null)
            {
                // default, when Draw.Interval and Update.Interval are unset
                targetElapsed = 1d / 60d;
            }
            else
            {
                targetElapsed = double.MaxValue;
                if (Draw.Interval != null)
                {
                    targetElapsed = Draw.Interval.Value;
                }
                if (Update.Interval != null)
                {
                    targetElapsed = Math.Min(Update.Interval.Value, targetElapsed);
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
