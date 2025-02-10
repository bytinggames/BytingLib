using YamlDotNet.Core.Tokens;

namespace BytingLib.UI
{
    public class DropDownList : PanelStack
    {
        private readonly Canvas canvas;
        private bool mouseDown;

        public DropDownList(Canvas canvas)
        {
            this.canvas = canvas;
            //Width = 0f;
            //Height = 0f;
            //Color = Microsoft.Xna.Framework.Color.Yellow;
        }

        protected override void UpdateSelf(ElementInput input)
        {
            if (!input.Input.Click.Down)
            {
                if (mouseDown)
                {
                    Hide(input);
                }
            }

            if (input.Input.Click.Pressed)
            {
                mouseDown = true;
                if (!Children.Any(f => f.AbsoluteRect.CollidesWith(input.Input.MousePosition)))
                {
                    Hide(input);
                }
            }
            else if (input.Input.Click.Released)
            {
                Hide(input);
            }

            base.UpdateSelf(input);
        }

        private void Hide(ElementInput input)
        {
            mouseDown = false;
            input.UnsetUpdateCatch(this);
            canvas.Remove(this);
        }
    }

    public class DropDown : ButtonParent
    {
        /// <summary>Used for stuff that should happen immediately, like sound effects</summary>
        public event Action? OnBeforeClick;
        public event Action? OnClick;
        public event Action<int, object>? OnSelect;

        private Element? dropDownPanel;
        private readonly Func<(string Text, object Obj)[]> getOptions;
        private (string Text, object Obj)[]? options;
        private readonly Canvas canvas;
        public Label Label { get; }

        public DropDown(string text, Func<(string Text, object Obj)[]> getOptions, Canvas canvas, float width, float height, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, anchor, padding)
        {
            this.getOptions = getOptions;
            this.canvas = canvas;

            Add(Label = new Label(text));
            Label.Anchor = new Vector2(0f, 0.5f);
        }

        private void SelectOption(int index, (string Text, object Obj) option)
        {
            Label.Text = option.Text;
            OnSelect?.Invoke(index, option.Obj);
        }

        protected override void DoClick()
        {
            Click();

            if (dropDownPanel == null)
            {
                dropDownPanel = CreateDropDownPanel();
            }

            dropDownPanel.Style = Style;
            dropDownPanel.Padding = new(AbsoluteRect.Left, AbsoluteRect.Bottom, 0f, 0f);
            canvas.Add(dropDownPanel);
            canvas.SetUpdateCatch(dropDownPanel);
        }

        private Element CreateDropDownPanel()
        {
            if (options == null)
            {
                options = getOptions();
            }

            var panel = new DropDownList(canvas);
            for (int i = 0; i < options.Length; i++)
            {
                int iRemember = i;
                panel.Add(
                    new Button(() => SelectOption(iRemember, options[iRemember]), Width, Height, Anchor)
                    {
                        HoverStyle = HoverStyle
                    }.Add(
                        new Label(options[i].Text) { Anchor = Label.Anchor }
                    )
                );
            }
            panel.Anchor = Vector2.Zero;

            return panel;
        }

        public void Click()
        {
            OnBeforeClick?.Invoke();
            OnClick?.Invoke();
        }
    }
}
