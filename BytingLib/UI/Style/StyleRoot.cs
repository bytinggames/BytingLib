﻿namespace BytingLib.UI
{
    public class StyleRoot
    {
        public Ref<SpriteFont> Font => GetOverride(f => f.Font)!;
        public Ref<SpriteFont>? FontBold => GetOverride(f => f.FontBold);
        public Color? FontColor => GetOverride(f => f.FontColor);
        public Color? FontBoldColor => GetOverride(f => f.FontBoldColor);
        public Vector2 FontScale => GetOverride(f => f.FontScale)!.Value;
        public Ref<Animation> ButtonAnimation => GetOverride(f => f.ButtonAnimation)!;
        public Padding? ButtonPadding => GetOverride(f => f.ButtonPadding);
        public bool ButtonPaddingToButtonBorder => GetOverride(f => f.ButtonPaddingToButtonBorder)!.Value;
        public float RoundPositionTo => GetOverride(f => f.RoundPositionTo)!.Value;

        public StyleBase StyleBase { get; set; }
        private List<Style> styleOverrides = new();
        public double TotalMilliseconds { get; set; }

        public float LineSpacing => Font.Value.LineSpacing * FontScale.Y;

        public StyleRoot(StyleBase baseStyle)
        {
            StyleBase = baseStyle;
            styleOverrides.Add(baseStyle);
        }

        private T? GetOverride<T>(Func<Style, T?> get)
        {
            for (int i = styleOverrides.Count - 1; i >= 0; i--)
            {
                T? value = get(styleOverrides[i]);
                if (value != null)
                {
                    return value;
                }
            }
            return default;
        }

        public void Push(Style? styleOverride)
        {
            if (styleOverride == null)
            {
                return;
            }

            styleOverrides.Add(styleOverride);
        }

        public void Pop(Style? styleOverride)
        {
            if (styleOverride == null)
            {
                return;
            }

            styleOverrides.RemoveAt(styleOverrides.Count - 1);
        }

        public Vector2 MeasureString(string text)
        {
            return Font.Value.MeasureString(text) * FontScale;
        }
    }
}
