
using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class UpdateKeyPressed : IUpdate, IUpdateWhenBelowPopup
    {
        private readonly KeyInput keys;
        private readonly Keys key;
        private readonly Action action;
        private readonly bool alsoUpdateBelowPopup;

        public UpdateKeyPressed(KeyInput keys, Keys key, Action action, bool alsoUpdateBelowPopup = false)
        {
            this.keys = keys;
            this.key = key;
            this.action = action;
            this.alsoUpdateBelowPopup = alsoUpdateBelowPopup;
        }

        public void Update()
        {
            if (keys.GetKey(key).Pressed)
            {
                action();
            }
        }

        public void UpdateWhenBelowPopup(Scene popup)
        {
            if (alsoUpdateBelowPopup)
            {
                Update();
            }
        }
    }
}
