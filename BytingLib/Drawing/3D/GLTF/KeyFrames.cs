namespace BytingLib
{
    public class KeyFrames
    {
        public float[] seconds; // in seconds

        public KeyFrames(byte[] bytes)
        {
            seconds = ByteExtension.ByteArrayToStructArray<float>(bytes, sizeof(float));
        }
    }
}
