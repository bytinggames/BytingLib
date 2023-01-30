namespace BytingLib
{
    public abstract class SamplerOutput
    {
        public abstract void Apply(NodeGL targetNode, int index);
        public abstract void Apply(NodeGL targetNode, int index1, int index2, float lerp);
    }
}
