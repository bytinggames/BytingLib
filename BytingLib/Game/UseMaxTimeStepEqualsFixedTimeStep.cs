using System.Diagnostics;

namespace BytingLib
{
    /// <summary>
    /// Used for when we know the game lags (f.ex. when a loading screen is happening) and we don't want to catch up with the updates afterwards
    /// </summary>
    public class UseMaxTimeStepEqualsFixedTimeStep : IDisposable, IUpdate
    {
        private GameWrapper gameWrapper;
        private TimeSpan? forTime;
        private bool disposed;
        private Stopwatch? sw;

        public UseMaxTimeStepEqualsFixedTimeStep(GameWrapper gameWrapper, TimeSpan? forTime = null)
        {
            this.gameWrapper = gameWrapper;
            this.forTime = forTime;
            gameWrapper.UseMaxTimeStepEqualsFixedTimeStep++;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                gameWrapper.UseMaxTimeStepEqualsFixedTimeStep--;
                sw = null;
            }
        }

        public void Update()
        {
            if (forTime != null)
            {
                if (sw == null)
                {
                    sw = new();
                    sw.Start();
                }
                else
                {
                    if (sw.Elapsed >= forTime.Value)
                    {
                        Dispose();
                        forTime = null;
                    }
                }
            }
        }
    }
}
