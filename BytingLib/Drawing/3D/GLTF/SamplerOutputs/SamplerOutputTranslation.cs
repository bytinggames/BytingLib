namespace BytingLib
{
    class SamplerOutputTranslation : SamplerOutputVector3
    {
        public SamplerOutputTranslation(byte[] bytes) : base(bytes)
        { }

        public override void Apply(NodeGL targetNode, int index)
        {
            targetNode.SetTranslation(vectors[index]);
        }

        public override void Apply(NodeGL targetNode, int index1, int index2, float lerp)
        {
            Vector3 q = Vector3.Lerp(vectors[index1], vectors[index2], lerp);
            targetNode.SetTranslation(q);
        }
    }
}
