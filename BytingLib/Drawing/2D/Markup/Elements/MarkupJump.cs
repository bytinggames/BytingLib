namespace BytingLib.Markup
{
    [MarkupShortcut("jump")]
    public class MarkupJump : ILeaf
    {
        public bool ConfinesToLineSpacing => true;

        public float X { get; }
        public float Y { get; }

        //public MarkupJump(float x, float y)
        //{
        //    X = x;
        //    Y = y;
        //}
        public MarkupJump(Vector2 jump)
        {
            X = jump.X;
            Y = jump.Y;
        }

        public void Draw(MarkupSettings settings)
        {
            //settings.Anchor = intoRectangle.GetAnchor(settings.Anchor.OX, settings.Anchor.OY);
            settings.Anchor.X = X;
            settings.Anchor.Y = Y;
            settings.Anchor.OY = 0f;
            settings.Anchor.OX = 0f;
        }

        public Vector2 GetSize(MarkupSettings settings, int start, int end)
        {
            return Vector2.Zero;
        }

        public override string ToString()
        {
            return "#jump";
            //return $"#jump(${X}|${Y})";
        }

        public void Dispose()
        {
        }
    }
}
