using Microsoft.Xna.Framework;
using System;

namespace BytingLib
{
    public class BufferWrapper<T>
    {
        private IMultiBuffer<T>? _lineBuffer;
        private readonly Func<IMultiBuffer<T>> setValue;
        private readonly Func<bool> canAdd;

        public BufferWrapper(Func<IMultiBuffer<T>> setValue, Func<bool> canAdd)
        {
            this.setValue = setValue;
            this.canAdd = canAdd;
        }

        private IMultiBuffer<T> Buffer => _lineBuffer ??= setValue();
        public void Draw(T t, Color color)
        {
            if (!canAdd())
                throw new BytingException("can't add primitive right now. Call PrimitiveRenderer.Begin() first");
            Buffer.Add(t, color);
        }
    }
}
