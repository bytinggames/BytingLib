namespace BytingLib
{
    public class Scene : StuffDisposable, IUpdate, IDrawBatch
    {
        public Scene? PopupScene { get; protected set; }
        public bool DrawUnderlyingParents { get; set; } = true;

        public event Action<Scene>? OnPopupOpen;
        public event Action? OnPopupClose;

        public Scene(params Type[] extraTypes)
            : base(new Type[] { typeof(IDraw), typeof(IUpdate), typeof(IUpdateWhenBelowPopup), typeof(IDrawBatch) }.Concat(extraTypes).ToArray())
        { }

        protected virtual void Begin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        protected virtual void DrawLoop(SpriteBatch spriteBatch)
        {
            ForEach<IDraw>(f => f.Draw(spriteBatch));
        }

        public void DrawBatch(SpriteBatch spriteBatch)
        {
            if (IsVisible())
            {
                DrawBatchInner(spriteBatch);
            }

            PopupScene?.DrawBatch(spriteBatch);
        }

        protected virtual void DrawBatchInner(SpriteBatch spriteBatch)
        {
            Begin(spriteBatch);

            DrawLoop(spriteBatch);

            spriteBatch.End();

            ForEach<IDrawBatch>(f => f.DrawBatch(spriteBatch));
        }

        public virtual void Update()
        {
            if (PopupScene != null)
            {
                PopupScene.Update();
                ForEach<IUpdateWhenBelowPopup>(f => f.UpdateWhenBelowPopup());
            }
            else
            {
                ForEach<IUpdate>(f => f.Update());
            }
        }

        public void SetPopupScene(Scene? scene)
        {
            if (PopupScene != null)
                RemovePopupScene();

            PopupScene = scene;

            if (PopupScene != null)
                OnPopupOpen?.Invoke(PopupScene);
        }

        public void RemovePopupScene()
        {
            if (PopupScene != null)
            {
                PopupScene?.Dispose();
                PopupScene = null;

                OnPopupClose?.Invoke();
            }
        }

        public Scene GetTopmostScene()
        {
            if (PopupScene == null)
            {
                return this;
            }
            return PopupScene.GetTopmostScene();
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
