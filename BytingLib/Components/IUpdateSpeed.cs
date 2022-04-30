using Microsoft.Xna.Framework;

namespace BytingLib
{
    public interface IUpdateSpeed
    {
        float Factor { get; }
        GameTime GameTime { get; }

        void Update(GameTime gameTime);
    }
}