namespace BytingLib
{
    abstract class SamplerOutputQuaternion : SamplerOutput
    {
        protected Quaternion[] rotations;

        public SamplerOutputQuaternion(byte[] bytes)
        {
            rotations = ByteExtension.ByteArrayToStructArray<Quaternion>(bytes, 4 * 4);
        }
    }
}
