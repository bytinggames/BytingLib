namespace BytingLib
{
    public struct HSVColor
    {
        private float _hue, _saturation, _value;
        public byte Alpha;

        public HSVColor(float hue, float saturation, float value, byte alpha = 255)
        {
            this._hue = hue;
            this._saturation = saturation;
            this._value = value;
            this.Alpha = alpha;
        }

        public HSVColor SetHSVA(float _h, float _s, float _v, byte _a)
        {
            _hue = _h;
            _saturation = _s;
            _value = _v;
            Alpha = _a;
            return this;
        }

        public float hue
        {
            get { return _hue; }
            set
            {
                if (value < 0f)
                {
                    _hue = (360f + value) % 360f;
                }
                else
                {
                    _hue = value % 360f;
                }
            }
        }

        public float saturation
        {
            get { return _saturation; }
            set
            {
                if (value > 1f)
                {
                    _saturation = 1f;
                }
                else if (value < 0f)
                {
                    _saturation = 0f;
                }
                else
                {
                    _saturation = value;
                }
            }
        }

        public float value
        {
            get { return _value; }
            set
            {
                if (value > 1f)
                {
                    _value = 1f;
                }
                else if (value < 0f)
                {
                    _value = 0f;
                }
                else
                {
                    _value = value;
                }
            }
        }

        public HSVColor SetHue(float _hue)
        {
            hue = _hue;
            return this;
        }
        public HSVColor SetValue(float _value)
        {
            value = _value;
            return this;
        }
        public HSVColor SetSaturation(float _saturation)
        {
            saturation = _saturation;
            return this;
        }
        public HSVColor AddHue(float _hue)
        {
            hue += _hue;
            return this;
        }
        public HSVColor AddValue(float _value)
        {
            value += _value;
            return this;
        }
        public HSVColor AddSaturation(float _saturation)
        {
            saturation += _saturation;
            return this;
        }
        public HSVColor TimesHue(float _hue)
        {
            hue *= _hue;
            return this;
        }
        public HSVColor TimesValue(float _value)
        {
            value *= _value;
            return this;
        }
        public HSVColor TimesSaturation(float _saturation)
        {
            saturation *= _saturation;
            return this;
        }

        public Color ToRGB()
        {
            Color color;

            float c = value * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = value - c;

            float r, g, b;
            r = g = b = 0;

            if (hue < 60)
            {
                r = c;
                g = x;
            }
            else if (hue < 120)
            {
                g = c;
                r = x;
            }
            else if (hue < 180)
            {
                g = c;
                b = x;
            }
            else if (hue < 240)
            {
                b = c;
                g = x;
            }
            else if (hue < 300)
            {
                b = c;
                r = x;
            }
            else if (hue < 360)
            {
                r = c;
                b = x;
            }
            else
            { }

            color = new Color((byte)(255 * (r + m)), (byte)(255 * (g + m)), (byte)(255 * (b + m)), Alpha);
            return color;
        }
    }
}
