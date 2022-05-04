using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace LevelSketch
{
    public class DepthLayers : IDepthGetter
    {
        private PropertyInfo[] properties;

        Stack<DepthLayer> layersInUse = new Stack<DepthLayer>();

        public float GetDepth()
        {
            if (layersInUse.Count == 0)
                return 0f;
            DepthLayer layer = layersInUse.Peek();
            return layer.GetDepth();
        }

        public DepthLayers()
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
