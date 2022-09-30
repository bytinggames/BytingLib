using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace BytingLib
{
    public class DepthLayers : IDepthGetter
    {
        private PropertyInfo[] properties;

        DepthLayer defaultLayer;
        Stack<DepthLayer> layersInUse = new Stack<DepthLayer>();

        public float GetDepth()
        {
            if (layersInUse.Count == 0)
                return defaultLayer.GetDepth();
            DepthLayer layer = layersInUse.Peek();
            return layer.GetDepth();
        }

        public DepthLayers()
        {
            defaultLayer = new DepthLayer(0f, this);

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

        internal void OnUseLayer(DepthLayer layer)
        {
            layersInUse.Push(layer);
        }

        internal void OnUnuseLayer()
        {
            layersInUse.Pop();
        }
    }
}
