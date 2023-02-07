namespace BytingLib
{
    public class Instances<InstanceVertex> : IInstances<InstanceVertex> where InstanceVertex : struct, IVertexType
    {
        SimpleArrayList<InstanceVertex> instances = new();

        /// <summary>Warning: don't forget to transpose the transform matrix</summary>
        public void AddTransposed(InstanceVertex instance)
        {
            instances.Add(instance);
        }

        public void Clear()
        {
            instances.Clear();
        }

        public int Count => instances.Count;

        public InstanceVertex[] Array => instances.Arr;
    }
}
