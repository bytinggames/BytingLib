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

        public override bool ConfinesToLineSpacing => scale == Vector2.One ? base.ConfinesToLineSpacing : false;

        public MarkupSpan(string str)
            : base(new ScriptReaderLiteral(str))
        {
        }

        protected override Vector2 GetSizeChildUnscaled(MarkupSettings settings, int start, int end)
        {
            return base.GetSizeChildUnscaled(settings, start, end) * scale;
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
