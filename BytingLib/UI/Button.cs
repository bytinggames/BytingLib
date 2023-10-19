namespace BytingLib.UI
{
    public class Button : ButtonParent
    {
        private readonly Action clickAction;

        public Button(Action clickAction, float width = 0, float height = 0, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, anchor, padding)
        {
            this.clickAction = clickAction;
        }

        protected override void OnClick()
        {
            clickAction();
        }
    }
}
