﻿namespace BytingLib.UI
{
    public class Checkbox : ButtonParent
    {
        private readonly Action<bool> onStateChanged;
        private Element? childUnchecked;
        private Element? childChecked;
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    Toggle();
                }
            }
        }
        public bool DisposeCheckpointChildrenOnDispose { get; set; } = true;


        public Checkbox(Action<bool> onStateChanged, bool startChecked, float width = 0, float height = 0, Vector2? anchor = null, Padding? padding = null)
            : base(width, height, anchor, padding)
        {
            this.onStateChanged = onStateChanged;
            _checked = startChecked;

        }

        public Checkbox InitCheckbox(Element childChecked, Element childUnchecked)
        {
            RemoveAndDisposeCheckpointChildren();

            this.childUnchecked = childUnchecked;
            this.childChecked = childChecked;

            if (_checked)
            {
                Add(childChecked);
            }
            else
            {
                Add(childUnchecked);
            }

            return this;
        }

        public Checkbox InitCheckboxTex(string textureKey, Creator creator)
        {
            var childChecked = new LabelMarkup($"#tex({textureKey})", creator);
            var childUnchecked = new LabelMarkup($"#tex({textureKey}0)", creator);
            return InitCheckbox(childChecked, childUnchecked);
        }

        private void RemoveAndDisposeCheckpointChildren()
        {
            if (childUnchecked != null)
            {
                Remove(childUnchecked);
                childUnchecked.Dispose();
            }
            if (childChecked != null)
            {
                Remove(childChecked);
                childChecked.Dispose();
            }
        }

        protected override void OnClick()
        {
            Toggle();
        }

        private void Toggle()
        {
            _checked = !_checked;

            if (_checked)
            {
                if (childUnchecked != null)
                {
                    Remove(childUnchecked);
                }
                if (childChecked != null)
                {
                    Add(childChecked);
                }
            }
            else
            {
                if (childChecked != null)
                {
                    Remove(childChecked);
                }
                if (childUnchecked != null)
                {
                    Add(childUnchecked);
                }
            }
            SetDirty();

            onStateChanged(_checked);
        }

        protected override void DisposeSelf()
        {
            if (DisposeCheckpointChildrenOnDispose)
            {
                RemoveAndDisposeCheckpointChildren();
            }

            base.DisposeSelf();
        }
    }
}
