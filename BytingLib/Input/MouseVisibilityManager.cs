using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class MouseVisibilityManager : IDisposable
    {
        private readonly IMouseVisible mouseVisible;
        private readonly IResolution res;
        private readonly bool setVisibleToOnDispose;

        public bool CenterIfAppearing { get; set; } = true;
        public bool AllowSetMousePos { get; set; } = true;

        public event Action? OnAppear, OnHide;

        public MouseVisibilityManager(IMouseVisible mouseVisible, IResolution res)
        {
            this.mouseVisible = mouseVisible;
            this.res = res;
            setVisibleToOnDispose = mouseVisible.IsMouseVisible;
        }

        public void Dispose()
        {
            SetVisibleTo(setVisibleToOnDispose);
        }

        public void UpdateEnd(Scene? topMostScene)
        {
            if (topMostScene == null)
            {
                return;
            }

            bool visibleSetTo = !topMostScene.HideMouse;

            if (mouseVisible.IsMouseVisible != visibleSetTo)
            {
                SetVisibleTo(visibleSetTo);
            }
        }

        private void SetVisibleTo(bool newVisibleState)
        {
            if (newVisibleState)
            {
                ShowInternal();
            }
            else
            {
                HideInternal();
            }

            mouseVisible.IsMouseVisible = newVisibleState;
        }

        private void ShowInternal()
        {
            if (CenterIfAppearing)
            {
                // only set mouse position, when not replaying
                if (AllowSetMousePos)
                {
                    var center = res.Resolution / 2;
                    Mouse.SetPosition(center.X, center.Y);
                }
            }

            OnAppear?.Invoke();
        }

        private void HideInternal()
        {
            OnHide?.Invoke();
        }
    }
}