namespace BytingLib.Markup
{
    [MarkupShortcut("jump")]
    public class MarkupJump : ILeaf
    {
        public bool ConfinesToLineSpacing => true;

        public float X { get; }
        public float Y { get; }

        public MarkupJump(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Draw(MarkupSettings settings)
        {
            settings.Anchor.X = X;
            settings.Anchor.Y = Y;
            settings.Anchor.OY = 0f;
            settings.Anchor.OX = 0f;
        }

        public Vector2 GetSize(MarkupSettings settings)
        {
            return Vector2.Zero;
        }

        public override string ToString()
        {
            return $"#jump(${X}|${Y})";
        }

        public void Dispose()
        {
        }
    }
}
