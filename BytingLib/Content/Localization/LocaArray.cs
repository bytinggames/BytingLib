namespace BytingLib
{
    public class LocaArray
    {
        private readonly Func<string, string> getLocaValue;
        private readonly string baseKey;
        public int Length { get; }

        public LocaArray(Func<string, string> getLocaValue, string baseKey, int length)
        {
            this.getLocaValue = getLocaValue;
            this.baseKey = baseKey;
            Length = length;
        }

        public string this[int index] => getLocaValue(baseKey + index);

        public IEnumerable<string> GetElements()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }
    }
}
