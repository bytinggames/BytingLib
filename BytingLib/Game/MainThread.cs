using System.Collections.Concurrent;
using System.Diagnostics;

namespace BytingLib
{
    public static class MainThread
    {
        private static ConcurrentQueue<Action> actionQueue = new();
        private static int MainThreadID = -1; // not specified yet. Set it via Initialize()
        private static Stopwatch executeStopwatch = new();

        public static void Initialize()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        private static void EnsureInitialized()
        {
            if (MainThreadID == -1)
            {
                throw new Exception("Call Initialize first");
            }
        }

        /// <summary>
        /// Adds an action to a queue that gets executed in the main thread.
        /// If this is the main thread, execute the action immediately instead.
        /// </summary>
        public static void Invoke(Action a)
        {
            EnsureInitialized();

            if (Thread.CurrentThread.ManagedThreadId == MainThreadID)
            {
                a();
            }
            else
            {
                actionQueue.Enqueue(a);
            }
        }

        /// <summary>
        /// Blocks this thread until the action has been executed on the main thread.
        /// If this is the main thread, execute the action immediately instead.
        /// </summary>
        public static void InvokeWaitUntilDone(Action a)
        {
            EnsureInitialized();

            if (Thread.CurrentThread.ManagedThreadId == MainThreadID)
            {
                a();
            }
            else
            {
                using (EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset))
                {
                    actionQueue.Enqueue(() =>
                    {
                        a();
                        waitHandle.Set();
                    });


                    waitHandle.WaitOne();
                }
            }
        }

        /// <summary>
        /// Blocks this thread until the action has been executed on the main thread.
        /// If this is the main thread, execute the action immediately instead.
        /// </summary>
        public static T InvokeWaitUntilDone<T>(Func<T> a)
        {
            EnsureInitialized();

            if (Thread.CurrentThread.ManagedThreadId == MainThreadID)
            {
                return a();
            }
            else
            {
                using (EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset))
                {
                    T? t = default;
                    actionQueue.Enqueue(() =>
                    {
                        t = a();
                        waitHandle.Set();
                    });


                    waitHandle.WaitOne();
                    return t!;
                }
            }
        }

        internal static void ExecuteActions(int targetMilliseconds)
        {
            executeStopwatch.Restart();
            while (actionQueue.TryDequeue(out Action? action))
            {
                action();

                if (executeStopwatch.ElapsedMilliseconds >= targetMilliseconds)
                {
                    break;
                }
            }
        }

        public static bool IsQueueEmpty()
        {
            return actionQueue.Count == 0;
        }
    }
}
