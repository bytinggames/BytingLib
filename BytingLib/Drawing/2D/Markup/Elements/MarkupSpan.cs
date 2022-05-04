using BytingLib.Creation;
using Microsoft.Xna.Framework;

namespace BytingLib.Markup
{
    [MarkupShortcut("span")]
    public class MarkupSpan : MarkupText
    {
        private Vector2 scale = Vector2.One;
        public Vector2 ScaleXY
        {
            get => scale;
            set => scale = value;
        }

        public float Scale
        {
            set => scale = new Vector2(value);
        }
        public float ScaleX
        {
            set => scale.X = value;
        }
        public float ScaleY
        {
            set => scale.Y = value;
        }

        public MarkupSpan(string str)
            : base(new ScriptReader(str))
        {
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings)
        {
            return base.GetSizeChildUnscaled(settings) * scale;
        }

        protected override void DrawChild(MarkupSettings settings)
        {
            Vector2 storeScale = settings.Scale;
            settings.Scale *= scale;
            base.DrawChild(settings);
            settings.Scale = storeScale;
        }
    }
}
