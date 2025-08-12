namespace BytingLib
{
    public class LocaArray
    {
        private readonly Dictionary<string, string> locaDict;
        private readonly string baseKey;
        public int Length { get; }

        public LocaArray(Dictionary<string, string> getLocaValue, string baseKey, int length)
        {
            this.locaDict = getLocaValue;
            this.baseKey = baseKey;
            Length = length;
        }

        public string this[int index] => locaDict[baseKey + index];

        public IEnumerable<string> GetElements()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        public bool ContainsIndex(int index)
        {
            return locaDict.ContainsKey(baseKey + index);
        }
    }
}
