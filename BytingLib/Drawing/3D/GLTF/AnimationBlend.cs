namespace BytingLib
{
    internal class AnimationBlend
    {
        public int animationId = 0;
        public int interpolationStep;
        int channelCountOnBlendBegin;
        SimpleArrayList<AnimationGL.Channel>? channels;

        internal void Begin()
        {
            if (channels == null)
                channels = new();
            else
                channels.Clear();
            animationId++;
            interpolationStep = 0;
        }

        internal bool AddChannelfNotAlready(AnimationGL.Channel channel)
        {
            if (channels == null)
                throw new Exception("Call Begin() first before calling AddChannel()");

            bool newChannel = channel.Target.AnimationBlendId != animationId;

            if (newChannel)
            {
                channels.Add(channel);
                channel.Target.AnimationBlendId = animationId;
            }

            channel.Target.AnimationBlendStep = interpolationStep;

            return newChannel;
        }

        internal void BeginAnimationBlend()
        {
            if (channels == null)
                throw new Exception("Call Begin() first before calling BeginAnimationBlend()");

            interpolationStep++;
            channelCountOnBlendBegin = channels.Count;
        }
        internal void EndAnimationBlend(float interpolationAmount)
        {
            if (channels == null)
                throw new Exception("Call Begin() and BeginAnimationBlend() first before calling EndAnimationBlend()");


            for (int i = 0; i < channelCountOnBlendBegin; i++) // only check old channels
            {
                if (channels[i].Target.AnimationBlendStep < interpolationStep)
                {
                    // do interpolation to default
                    channels[i].BlendToDefault(interpolationAmount);
                    channels[i].Target.AnimationBlendStep = interpolationStep;
                }
            }
        }
    }
}
