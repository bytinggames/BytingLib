namespace BytingLib
{
    public class AnimationInstance
    {
        public int Animation { get; set; }
        public float StartTimeStamp { get; set; }

        public AnimationInstance(int animation, float timeStamp)
        {
            Animation = animation;
            StartTimeStamp = timeStamp;
        }
    }
}
