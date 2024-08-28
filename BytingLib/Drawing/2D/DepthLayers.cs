using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BytingLib
{
    public class DepthLayers : IDepthGetter
    {
        private PropertyInfo[] properties;

        protected IDepthLayer defaultLayer;
        Stack<IDepthLayer> layersInUse = new Stack<IDepthLayer>();

        public virtual float GetDepth()
        {
            if (layersInUse.Count == 0)
            {
                return defaultLayer.GetDepth();
            }

            IDepthLayer layer = layersInUse.Peek();
            return layer.GetDepth();
        }

        public DepthLayers()
        {
            defaultLayer = new DepthLayer(0f, this);

            Initialize();
        }

        public DepthLayers(IDepthLayer defaultLayer)
        {
            this.defaultLayer = defaultLayer;

            Initialize();
        }

        [MemberNotNull(nameof(properties))]
        private void Initialize()
        {
            properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            ResetLayers();
        }

        public void ResetLayers()
        {
            int count = properties.Length;
            for (int i = 0; i < properties.Length; i++)
            {
                float index = (float)i / count;
                properties[i].SetValue(this, new DepthLayer(index, this));
            }

            layersInUse.Clear();
        }

        internal void OnUseLayer(IDepthLayer layer)
        {
            layersInUse.Push(layer);
        }

        internal void OnUnuseLayer()
        {
            layersInUse.Pop();
        }
    }
}
