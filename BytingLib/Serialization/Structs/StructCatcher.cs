namespace BytingLib.Serialization
{
    /// <summary>
    /// Removes flags if matching.
    /// </summary>
    public class StructCatcher<T> : IStructListener<T> where T : struct
    {
        private readonly byte[] catchBytes;

        public StructCatcher(T catchMask)
        {
            catchBytes = StructSerializer.GetBytes(catchMask);
        }

        public virtual bool DoesMatch(byte[] stateBytes)
        {
            if (stateBytes.Length != catchBytes.Length)
            {
                return false;
            }

            for (int j = 0; j < stateBytes.Length; j++)
            {
                if ((stateBytes[j] & catchBytes[j]) != catchBytes[j])
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void OnMatch(byte[] stateBytes)
        {
            for (int i = 0; i < catchBytes.Length; i++)
            {
                if (catchBytes[i] != 0)
                {
                    stateBytes[i] = (byte)(stateBytes[i] ^ catchBytes[i]);
                }
            }
        }
    }
}
