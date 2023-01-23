using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IRenderBuffer
    {
        public void Clear();
        public int Count { get; }
        public void Draw(DynamicVertexBuffer instanceBuffer, Effect effect);
        public void Draw(DynamicVertexBuffer instanceBuffer, IShader shader);
        public void Dispose();
    }
    public interface IRenderBuffer<T>
    {
        public void Add(T primitive, Color color);
    }
}
