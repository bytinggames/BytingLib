using System.Diagnostics;

namespace BytingLib
{
    public class Promise<T>
    {
        T? val;

        public T Value
        {
            get
            {
                if (val == null)
                {
#if DEBUG
                    Debug.WriteLine("Don't force loading the value of a promise. This slows loading down. Instead, let the code that is dependant on this value run on the main thread too via MainThread.Invoke()");
#endif
                    // wait until main thread populated this value
                    while (val == null)
                    {
                        Thread.Sleep(1);
                    }
                }
                return val;
            }
            set
            {
                val = value;
            }
        }

        public Promise(Func<T> doOnMainThread)
        {
            MainThread.Invoke(() => val = doOnMainThread());
        }

        public Promise(T val)
        {
            this.val = val;
        }
    }
}
