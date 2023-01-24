using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BytingLib
{
    public interface IShaderTexWorld : IShader
    {
        EffectParameterStack<Matrix> World { get; }
        EffectParameterStack<Texture2D> ColorTex { get; }
    }
}
