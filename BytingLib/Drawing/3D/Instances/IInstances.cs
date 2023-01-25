namespace BytingLib
{
    public interface IInstances<InstanceVertex> where InstanceVertex : struct, IVertexType
    {
        void Add(InstanceVertex instance);
        void Clear();
        int Count { get; }
        InstanceVertex[] Array { get; }
    }
}
