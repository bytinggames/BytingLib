namespace BytingLib
{
    internal class DictionaryCacheChannelTargets
    {
        private readonly ModelGL model;
        private readonly Dictionary<uint, AnimationGL.ChannelTarget> dictionary = new();

        public DictionaryCacheChannelTargets(ModelGL model)
        {
            this.model = model;
        }

        public AnimationGL.ChannelTarget Get(int targetNodeIndex, string path)
        {
            uint number = path[0] switch
            {
                't' => 0,
                'r' => 1,
                's' => 2,
                'w' => 3,
                _ => throw new NotImplementedException()
            };

            uint key = (uint)targetNodeIndex * 4 + number;

            AnimationGL.ChannelTarget? channelTarget;
            if (dictionary.TryGetValue(key, out channelTarget))
                return channelTarget;

            channelTarget = new(model, targetNodeIndex);

            dictionary.Add(key, channelTarget);
            return channelTarget;
        }
    }
}
