
namespace BytingLib
{
    public class DepthLayer
    {
        private float depth;
        private readonly DepthLayers parent;

        public DepthLayer(float depth, DepthLayers parent)
        {
            this.depth = depth;
            this.parent = parent;
        }

        public float GetDepth()
        {
            depth = depth.GetIncrement();
            return depth;
        }

        public static implicit operator float(DepthLayer layer)
        {
            return layer.GetDepth();
        }

        public IDisposable Use()
        {
            parent.OnUseLayer(this);
            return new LayerUsage(this);
        }

        private void OnDisposeLayerUsage()
        {
            parent.OnUnuseLayer();
        }

        struct LayerUsage : IDisposable
        {
            private DepthLayer layer;

            public LayerUsage(DepthLayer layer)
            {
                this.layer = layer;
            }

            public void Dispose()
            {
                layer.OnDisposeLayerUsage();
            }
        }
    }
}
