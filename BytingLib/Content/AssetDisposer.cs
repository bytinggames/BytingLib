using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BytingLib
{
    public static class AssetDisposer
    {
        /// <summary>Calls IDisposable.Dispose() and SpriteFont.Texture.Dispose() if existing</summary>
        public static void Dispose(object? asset)
        {
            if (asset is IDisposable disposable)
                disposable.Dispose();
            if (asset is SpriteFont font)
                font.Texture.Dispose();
        }
    }
}
