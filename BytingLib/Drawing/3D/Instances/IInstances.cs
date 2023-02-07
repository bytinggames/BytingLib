namespace BytingLib
{
    public interface IInstances<InstanceVertex> where InstanceVertex : struct, IVertexType
    {
        void AddTransposed(InstanceVertex instance);
        void Clear();
        int Count { get; }
        InstanceVertex[] Array { get; }
    }
}
