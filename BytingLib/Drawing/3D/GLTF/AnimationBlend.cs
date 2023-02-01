namespace BytingLib
{
    public class AnimationBlend
    {
        internal AnimationBlend()
        { }

        public int interpolationStep = 0;
        int channelCountOnBlendBegin;
        List<AnimationGL.Channel> channels = new();

        public bool AddChannel(AnimationGL.Channel channel)
        {
            bool newChannel = channel.Target.CurrentAnimationBlend != this;

            if (newChannel)
            {
                channels.Add(channel);
                channel.Target.CurrentAnimationBlend = this;
            }

            channel.Target.CurrentAnimationBlendInterpolationStep = interpolationStep;

            return newChannel;
        }

        internal void BeginAddAnimationBlend()
        {
            interpolationStep++;
            channelCountOnBlendBegin = channels.Count;
        }
        internal void EndAddAnimationBlend(float interpolationAmount)
        {
            for (int i = 0; i < channelCountOnBlendBegin; i++) // only check old channels
            {
                if (channels[i].Target.CurrentAnimationBlendInterpolationStep < interpolationStep)
                {
                    // do interpolation to default
                    channels[i].BlendToDefault(interpolationAmount);
                    channels[i].Target.CurrentAnimationBlendInterpolationStep = interpolationStep;
                }
            }
        }
    }
}
