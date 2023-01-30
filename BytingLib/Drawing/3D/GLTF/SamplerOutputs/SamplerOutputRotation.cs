namespace BytingLib
{
    class SamplerOutputRotation : SamplerOutputQuaternion
    {
        public SamplerOutputRotation(byte[] bytes) : base(bytes)
        { }

        public override void Apply(NodeGL targetNode, int index)
        {
            targetNode.SetRotation(rotations[index]);
        }

        public override void Apply(NodeGL targetNode, int index1, int index2, float lerp)
        {
            Quaternion q = Quaternion.Lerp(rotations[index1], rotations[index2], lerp);
            targetNode.SetRotation(q);
        }
    }
}
