namespace BytingLib
{
    class DictionaryCacheKeyFrames
    {
        private readonly ModelGL model;
        private readonly Dictionary<int, KeyFrames> keyFrames = new();

        public DictionaryCacheKeyFrames(ModelGL model)
        {
            this.model = model;
        }

        public KeyFrames? Get(int accessorIndex)
        {
            KeyFrames? val;
            if (keyFrames.TryGetValue(accessorIndex, out val))
            {
                return val;
            }

            val = new KeyFrames(model.GetBytesFromBuffer(accessorIndex));

            keyFrames.Add(accessorIndex, val);
            return val;
        }
    }
}
