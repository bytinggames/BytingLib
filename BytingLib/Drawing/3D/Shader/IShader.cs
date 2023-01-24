using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IShader
    {
        Effect Effect { get; }

        void ApplyParameters();
        IDisposable UseTechnique(string technique);
    }
}
