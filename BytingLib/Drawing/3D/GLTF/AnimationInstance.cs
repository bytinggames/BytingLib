namespace BytingLib
{
    public class AnimationInstance
    {
        public AnimationGL Animation { get; set; }
        public float StartTimeStamp { get; set; }

        public AnimationInstance(AnimationGL animation, float timeStamp)
        {
            Animation = animation;
            StartTimeStamp = timeStamp;
        }
    }
}
