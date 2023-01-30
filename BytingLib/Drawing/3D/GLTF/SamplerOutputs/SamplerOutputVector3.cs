namespace BytingLib
{
    abstract class SamplerOutputVector3 : SamplerOutput
    {
        protected Vector3[] vectors;

        public SamplerOutputVector3(byte[] bytes)
        {
            vectors = ByteExtension.ByteArrayToStructArray<Vector3>(bytes, 3 * 4);
        }
    }
}
