using BytingLib.UI;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public class ColorPicker : PanelTexture
    {
        private readonly Color[] color = new Color[1];
        public event Action<Color>? OnColorHold, OnColorRelease;
        private bool catched;
        private Int2? selectedCoordinate;


        public ColorPicker(Ref<Texture2D> colorWheelTex, Color? color)
            : base(colorWheelTex)
        {
            OnHoverSustain += ColorPicker_OnHoverSustain;

            if (color != null)
            {
                var colors = colorWheelTex.Value.ToColor();
                int minIndex = colors.IndexOfMinBy(f => Math.Abs(color.Value.R - f.R) + Math.Abs(color.Value.G - f.G) + Math.Abs(color.Value.B - f.B));

                int x = minIndex % colorWheelTex.Value.Width;
                int y = minIndex / colorWheelTex.Value.Width;
                selectedCoordinate = new Int2(x, y);
            }
        }

        private bool ColorPicker_OnHoverSustain(Element element, ElementInput input)
        {
            if (input.Input.Click.Pressed)
            {
                input.SetUpdateCatch(this);
                catched = true;
                return true;
            }
            return false;
        }

        protected override void UpdateSelf(ElementInput input)
        {
            base.UpdateSelf(input);

            if (catched)
            {
                if (input.Input.Click.Down)
                {
                    if (UpdateCoord(input))
                    {
                        if (OnColorHold != null)
                        {
                            if (UpdateColor(input, selectedCoordinate.Value))
                            {
                                OnColorHold?.Invoke(color[0]);
                            }
                        }
                    }
                }
                else
                {
                    if (input.Input.Click.Released)
                    {
                        UpdateCoord(input);
                         
                        if (OnColorRelease != null && selectedCoordinate != null)
                        {
                            UpdateColor(input, selectedCoordinate.Value);

                            OnColorRelease?.Invoke(color[0]);
                        }
                    }

                    input.UnsetUpdateCatch(this);
                    catched = false;
                }
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch, StyleRoot style)
        {
            base.DrawSelf(spriteBatch, style);

            if (selectedCoordinate != null)
            {
                Vector2 coord = selectedCoordinate.Value.ToVector2() / Texture.Value.GetSize() * AbsoluteRect.Size + AbsoluteRect.Pos;

                spriteBatch.DrawCross(coord, 20f, 4f, Color.Black);
                spriteBatch.DrawCross(coord, 16f, 2f, Color.White);
            }
        }

        [MemberNotNullWhen(true, nameof(selectedCoordinate))]
        private bool UpdateCoord(ElementInput input)
        {
            Vector2 coord = input.Input.MousePosition - AbsoluteRect.Pos;
            coord /= AbsoluteRect.Size;
            coord *= Texture.Value.GetSize();
            Int2 coordInt = new Int2(coord);
            coordInt.SetInRect(0, 0, Texture.Value.Width - 1, Texture.Value.Height - 1);

            if (coordInt != selectedCoordinate)
            {
                selectedCoordinate = coordInt;
                return true;
            }
            return false;
        }

        private bool UpdateColor(ElementInput input, Int2 coord)
        {
            var oldColor = color[0];

            Texture.Value.GetData(0, new Rectangle(coord.X, coord.Y, 1, 1), color, 0, 1);

            return oldColor != color[0];
        }
    }
}
