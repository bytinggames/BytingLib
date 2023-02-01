namespace BytingLib
{
    public class AnimationBlend
    {
        class ChannelAndStep
        {
            public AnimationGL.Channel Channel;
            public int InterpolationStep;

            public ChannelAndStep(AnimationGL.Channel channel, int interpolationStep)
            {
                Channel = channel;
                InterpolationStep = interpolationStep;
            }
        }

        internal AnimationBlend()
        { }

        public int interpolationStep = 0;
        int channelCountOnBlendBegin;
        List<ChannelAndStep> channels = new();

        public bool AddChannel(AnimationGL.Channel channel)
        {
            if (channel.Target.CurrentAnimationBlend != this)
            {
                channels.Add(new ChannelAndStep(channel, interpolationStep));
                channel.Target.CurrentAnimationBlend = this;
                return true;
            }
            else
            {
                // TODO: this is not nice
                var x = channels.Find(f => f.Channel.Target == channel.Target)!;
                x.InterpolationStep = interpolationStep;
            }
            return false;
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
                if (channels[i].InterpolationStep < interpolationStep)
                {
                    // do interpolation to default
                    channels[i].Channel.BlendToDefault(interpolationAmount);
                    channels[i].InterpolationStep = interpolationStep;
                }
            }
        }
    }
}
