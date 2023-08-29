using System.Diagnostics.CodeAnalysis;

namespace BytingLib.UI
{
    public class SliderInt : Element
    {
        private int _steps;
        public int Steps
        {
            get => _steps;
            [MemberNotNull(nameof(stepRects))]
            set
            {
                value = Math.Max(0, value);
                if (value != _steps || stepRects == null)
                {
                    _steps = value;
                    stepRects = new Rect[value];
                    for (int i = 0; i < stepRects.Length; i++)
                    {
                        stepRects[i] = new Rect();
                    }
                }
            }
        }
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value >= Steps)
                {
                    value = Steps - 1;
                }
                if (value != Value)
                {
                    _value = value;
                    OnValueChanged?.Invoke(value);
                }

                ValueUnset = false;
            }
        }
        public int KnobWidth { get; set; } = 16;
        private int StepLineWidth => KnobWidth / 8;
        private int LineHeight => StepLineWidth;
        public bool ValueUnset { get; set; } = true;
        private readonly Rect valueRect;
        private Rect[] stepRects;
        private readonly Rect lineRect;
        private bool catched, hover;
        public event Action<int>? OnValueChanged;
        private int valueBeforeMouseClick;
        public event Action<int>? OnValueChangedAfterMouseReleased;

        private float AbsoluteInnerLeft => AbsoluteRect.Left + KnobWidth / 2f;
        private float AbsoluteInnerRight => AbsoluteRect.Right - KnobWidth / 2f;
        private float AbsoluteInnerWidth => AbsoluteInnerRight - AbsoluteInnerLeft;


        public SliderInt(int steps)
        {
            Steps = steps;
            Width = -1f;
            Height = -1f;

            valueRect = new Rect();
            lineRect = new Rect();
        }

        protected override void UpdateSelf(ElementInput input)
        {
            hover = AbsoluteRect.CollidesWith(input.Mouse.Position);
            if (input.Mouse.Left.Pressed
                && hover)
            {
                input.SetUpdateCatch(this);
                catched = true;
                valueBeforeMouseClick = Value;
            }

            if (catched)
            {
                Value = (int)Math.Round((input.Mouse.X - AbsoluteInnerLeft) / AbsoluteInnerWidth * (Steps - 1));

                if (!input.Mouse.Left.Down)
                {
                    input.SetUpdateCatch(null);
                    catched = false;

                    if (valueBeforeMouseClick != Value)
                    {
                        OnValueChangedAfterMouseReleased?.Invoke(Value);
                    }
                }
            }

            base.UpdateSelf(input);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            if (style.FontColor != null)
            {
                lineRect.Pos = new Vector2(AbsoluteInnerLeft, AbsoluteRect.Y + AbsoluteRect.Height / 2f);
                lineRect.Y -= LineHeight / 2f;
                lineRect.Size.X = AbsoluteInnerWidth;
                lineRect.Size.Y = LineHeight;
                lineRect.Draw(spriteBatch, style.FontColor.Value * 0.5f);

                for (int i = 0; i < stepRects.Length; i++)
                {
                    stepRects[i].Pos = StepToPos(i) + new Vector2(-StepLineWidth / 2, AbsoluteRect.Height / 4f);
                    stepRects[i].Size.X = StepLineWidth;
                    stepRects[i].Size.Y = AbsoluteRect.Height / 2f;
                    stepRects[i].Draw(spriteBatch, style.FontColor.Value * 0.5f);
                }

                if (!ValueUnset)
                {
                    valueRect.Pos = StepToPos(Value) + new Vector2(-KnobWidth / 2, 0f);
                    valueRect.Size.X = KnobWidth;
                    valueRect.Size.Y = AbsoluteRect.Height;
                    valueRect.Draw(spriteBatch, style.FontColor.Value * (catched ? 1f : hover ? 0.75f : 0.5f));
                }
            }

            base.DrawSelf(spriteBatch, style);
        }

        private Vector2 StepToPos(int step)
        {
            float lerp = (float)step / (Steps - 1);
             return new Vector2(AbsoluteInnerLeft + lerp * AbsoluteInnerWidth, AbsoluteRect.Y);
        }
    }
}
