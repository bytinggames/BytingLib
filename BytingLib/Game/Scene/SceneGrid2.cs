namespace BytingLib
{
    public class SceneGrid2<T> : Scene, IEntityContainer<T> where T : IBoundingRect
    {
        public Grid2<T> Grid { get; }

        public SceneGrid2(float gridSize, params Type[] extraTypes)
            : base(extraTypes)
        {
            Grid = new Grid2<T>(gridSize);
        }

        public override void Remove(object thing)
        {
            if (thing is T t)
            {
                Grid.Remove(t);
            }

            base.Remove(thing);
        }

        public override void Add(object thing, Action<object>? onRemove = null)
        {
            base.Add(thing, onRemove);

            if (thing is T t)
            {
                Grid.Add(t);
            }
        }
    }
}
