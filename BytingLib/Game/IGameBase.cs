using Microsoft.Xna.Framework;

namespace BytingLib
{
    public interface IGameBase : IDisposable
    {
        void UpdateActive(GameTime gameTime);
        void UpdateInactive(GameTime gameTime);
        void DrawActive(GameTime gameTime);
        void OnActivate();
        void OnDeactivate();
        void DrawInactiveOnce();
    }
}
