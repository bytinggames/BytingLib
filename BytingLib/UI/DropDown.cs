using Microsoft.Xna.Framework.Input;
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
                    RemoveFromCanvas();
                }
            }

            if (input.Input.Click.Pressed)
            {
                mouseDown = true;
                if (!Children.Any(f => f.AbsoluteRect.CollidesWith(input.Input.MousePosition)))
                {
                    RemoveFromCanvas();
                }
            }
            else if (input.Input.Click.Released)
            {
                RemoveFromCanvas();
            }

            base.UpdateSelf(input);
        }

        public void RemoveFromCanvas()
        {
            mouseDown = false;
            canvas.Input.UnsetUpdateCatch(this);
            canvas.Remove(this);
        }
    }

    public class Dropdown : ButtonParent
    {
        /// <summary>Used for stuff that should happen immediately, like sound effects</summary>
        public event Action? OnBeforeClick;
        public event Action? OnClick;
        public event Action<int, object>? OnSelect;

        private DropDownList? dropDownPanel;
        private Button[]? buttons;
        private Label[]? labels;
        private readonly Func<(string Text, object Obj)[]> getOptions;
        private (string Text, object Obj)[]? options;
        private readonly Canvas canvas;
        public Label Label { get; }
        public float? ListItemHeight { get; set; }

        public Dropdown(string text, Func<(string Text, object Obj)[]> getOptions, Canvas canvas, float width, float height, Vector2? anchor = null, Padding? padding = null)
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

            // in case we press enter using ui navigation, we need to hide the panel
            dropDownPanel?.RemoveFromCanvas();
            if (canvas.NavigateElement != null)
            {
                canvas.NavigateElement = this;
            }
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

            // navigate to the right button in the dropdown list
            if (canvas.NavigateElement != null
                && labels != null
                && buttons != null)
            {
                int index = Array.FindIndex(labels, f => f.Text == Label.Text);
                if (index == -1)
                {
                    if (buttons.Length > 0)
                    {
                        canvas.NavigateElement = buttons[0];
                    }
                }
                else
                {
                    canvas.NavigateElement = buttons[index];
                }
            }
        }

        private DropDownList CreateDropDownPanel()
        {
            if (options == null)
            {
                options = getOptions();
            }

            var panel = new DropDownList(canvas);
            buttons = new Button[options.Length];
            labels = new Label[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                int iRemember = i;

                Button button = new Button(() => SelectOption(iRemember, options[iRemember]), Width, ListItemHeight ?? Height, Anchor)
                {
                    HoverStyle = HoverStyle
                };
                buttons[i] = button;
                panel.Add(
                    button.Add(
                        labels[i] = new Label(options[i].Text) { Anchor = Label.Anchor }
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
