namespace BytingLib.UI
{
    public class Button : ButtonParent
    {
        /// <summary>Used for stuff that should happen immediately, like sound effects</summary>
        public event Action? OnBeforeClick;
        public event Action OnClick;

        public Button(Action clickAction, float width = 0, float height = 0, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, anchor, padding)
        {
            this.OnClick = clickAction;
        }

        protected override void DoClick()
        {
            Click();
        }

        public void Click()
        {
            OnBeforeClick?.Invoke();
            OnClick();
        }
    }
}
