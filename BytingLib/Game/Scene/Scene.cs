namespace BytingLib
{
    public class Scene : StuffDisposable, IUpdate, IDrawBatch
    {
        public Scene? PopupScene { get; set; }

        public Scene(params Type[] extraTypes)
            : base(new Type[] { typeof(IDraw), typeof(IUpdate), typeof(IDrawBatch) }.Concat(extraTypes).ToArray())
        { }

        protected virtual void Begin(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        protected virtual void DrawLoop(SpriteBatch spriteBatch)
        {
            ForEach<IDraw>(f => f.Draw(spriteBatch));
        }

        public virtual void DrawBatch(SpriteBatch spriteBatch)
        {
            Begin(spriteBatch);

            DrawLoop(spriteBatch);

            spriteBatch.End();

            ForEach<IDrawBatch>(f => f.DrawBatch(spriteBatch));

            PopupScene?.DrawBatch(spriteBatch);
        }

        public virtual void Update()
        {
            if (PopupScene != null)
                PopupScene.Update();
            else
                ForEach<IUpdate>(f => f.Update());
        }
    }
}
