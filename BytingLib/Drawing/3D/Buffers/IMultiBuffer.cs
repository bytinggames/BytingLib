using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IMultiBuffer
    {
        public void Clear();
        public int Count { get; }
        public void Draw(DynamicVertexBuffer instanceBuffer, Effect effect);
        public void Dispose();
    }
    public interface IMultiBuffer<T>
    {
        public void Add(T primitive, Color color);
    }
}
