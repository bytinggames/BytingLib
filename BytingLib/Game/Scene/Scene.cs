namespace BytingLib
{
    public class Scene : StuffDisposable, IUpdate, IDrawBatch
    {
        public Scene? PopupScene { get; protected set; }
        public bool DrawUnderlyingParents { get; set; } = true;
        /// <summary>Enables calling Update() on Parent instead of UpdateBelowPopup()</summary>
        public bool UpdateUnderlyingParents { get; set; } = false;

        public event Action<Scene>? OnPopupOpen;
        public event Action<Scene>? OnBeforePopupClose;
        public Action? OnShowAsMainScene;

        public bool HideMouse { get; set; }

        public Scene(params Type[] extraTypes)
            : base(new Type[] { typeof(IDraw), typeof(IUpdate), typeof(IUpdateWhenBelowPopup), typeof(IDrawBatch) }.Concat(extraTypes).ToArray())
        { }

        protected virtual void DrawBegin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        protected virtual void DrawEnd(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected virtual void DrawLoop(SpriteBatch spriteBatch)
        {
            ForEach<IDraw>(f => f.Draw(spriteBatch));
        }

        public void DrawBatch(SpriteBatch spriteBatch, float extrapolation)
        {
            if (IsVisible())
            {
                DrawBatchInner(spriteBatch, extrapolation);
            }

            PopupScene?.DrawBatch(spriteBatch, extrapolation);
        }

        protected virtual void DrawBatchInner(SpriteBatch spriteBatch, float extrapolation)
        {
            DrawBegin(spriteBatch);

            DrawLoop(spriteBatch);

            DrawEnd(spriteBatch);

            ForEach<IDrawBatch>(f => f.DrawBatch(spriteBatch, extrapolation));
        }

        public virtual void Update()
        {
            if (PopupScene != null)
            {
                if (!PopupScene.UpdateUnderlyingParents)
                {
                    ForEach<IUpdateWhenBelowPopup>(f => f.UpdateWhenBelowPopup(PopupScene));
                }
                PopupScene.Update();

                if (PopupScene != null && PopupScene.UpdateUnderlyingParents)
                {
                    UpdateInner();
                }
            }
            else
            {
                UpdateInner();
            }
        }

        private void UpdateInner()
        {
            ForEach<IUpdate>(f => f.Update());
        }

        public void SetPopupScene(Scene? scene)
        {
            if (PopupScene != null)
            {
                RemovePopupScene();
            }

            PopupScene = scene;

            if (PopupScene != null)
            {
                OnPopupOpen?.Invoke(PopupScene);
            }
        }

        public void RemovePopupScene() => RemovePopupScene(true);

        public void RemovePopupScene(bool disposePopup)
        {
            if (PopupScene != null)
            {
                OnBeforePopupClose?.Invoke(PopupScene);

                if (disposePopup)
                {
                    PopupScene?.Dispose();
                }
                PopupScene = null;
            }
        }

        public bool RemovePopupSceneRecursively(Scene popupToRemove)
        {
            if (PopupScene == null)
            {
                return false;
            }
            if (PopupScene == popupToRemove)
            {
                RemovePopupScene();
                return true;
            }
            return PopupScene.RemovePopupSceneRecursively(popupToRemove);
        }

        public Scene GetTopmostScene()
        {
            if (PopupScene == null)
            {
                return this;
            }
            return PopupScene.GetTopmostScene();
        }

        public IEnumerable<Scene> GetPopupsRecusively(bool includingThis)
        {
            if (includingThis)
            {
                yield return this;
            }
            if (PopupScene == null)
            {
                yield break;
            }
            yield return PopupScene;

            foreach (var popup in PopupScene.GetPopupsRecusively(false))
            {
                yield return popup;
            }
        }

        public override void Dispose()
        {
            RemovePopupScene();

            base.Dispose();
        }

        private bool IsVisible()
        {
            if (PopupScene == null)
            {
                return true;
            }

            if (!PopupScene.DrawUnderlyingParents)
            {
                return false;
            }

            return PopupScene.IsVisible();
        }
    }
}
