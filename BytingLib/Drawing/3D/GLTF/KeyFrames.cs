namespace BytingLib
{
    public class KeyFrames
    {
        public float[] Seconds { get; set; } // in seconds

        public KeyFrames(byte[] bytes)
        {
            Seconds = ByteExtension.ByteArrayToStructArray<float>(bytes, sizeof(float));
        }
    }
}
