
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class UpdateKeyPressed : IUpdate
    {
        private readonly KeyInput keys;
        private readonly Keys key;
        private readonly Action action;

        public UpdateKeyPressed(KeyInput keys, Keys key, Action action)
        {
            this.keys = keys;
            this.key = key;
            this.action = action;
        }

        public void Update()
        {
            if (keys.GetKey(key).Pressed)
                action();
        }
    }
}
