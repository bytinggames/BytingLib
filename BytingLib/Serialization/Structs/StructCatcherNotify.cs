namespace BytingLib.Serialization
{
    /// <summary>
    /// Removes flags if matching and notifies on some events.
    /// </summary>
    public class StructCatcherNotify<T> : StructCatcher<T> where T : struct
    {
        public event Action? OnCatchBegin, OnCatchSustain, OnCatchEnd, OnCatchNone;

        private bool currentlyMatching;

        public StructCatcherNotify(T catchMask) : base(catchMask)
        {
        }

        public override bool DoesMatch(byte[] stateBytes)
        {
            bool match = base.DoesMatch(stateBytes);

            if (match)
            {
                if (currentlyMatching != match)
                    OnCatchBegin?.Invoke();
                else
                    OnCatchSustain?.Invoke();
            }
            else
            {
                if (currentlyMatching != match)
                    OnCatchEnd?.Invoke();
                else
                    OnCatchNone?.Invoke();
            }
            currentlyMatching = match;

            return match;
        }
    }
}
