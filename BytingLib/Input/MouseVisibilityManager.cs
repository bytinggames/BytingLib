using Microsoft.Xna.Framework.Input;

namespace BytingLib
{
    public class MouseVisibilityManager : IDisposable
    {
        private readonly IMouseVisible mouseVisible;
        private readonly bool setVisibleToOnDispose;

        public bool CenterIfAppearing { get; set; } = true;
        public bool AllowSetMousePos { get; set; } = true;

        public event Action? OnAppear, OnHide;

        public MouseVisibilityManager(IMouseVisible mouseVisible)
        {
            this.mouseVisible = mouseVisible;
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
                    var dispMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                    Mouse.SetPosition(dispMode.Width / 2, dispMode.Height / 2);
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