using Microsoft.Xna.Framework;
using System;

namespace BytingLib
{
    public class BufferWrapper<T>
    {
        private IRenderBuffer<T>? _lineBuffer;
        private readonly Func<IRenderBuffer<T>> setValue;
        private readonly Func<bool> canAdd;

        public BufferWrapper(Func<IRenderBuffer<T>> setValue, Func<bool> canAdd)
        {
            this.setValue = setValue;
            this.canAdd = canAdd;
        }

        private IRenderBuffer<T> Buffer => _lineBuffer ??= setValue();
        public void Draw(T t, Color color)
        {
            if (!canAdd())
                throw new BytingException("can't add primitive right now. Call PrimitiveRenderer.Begin() first");
            Buffer.Add(t, color);
        }
    }
}
