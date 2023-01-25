namespace BytingLib
{
    public interface IEntityContainer<T> where T : IBoundingRect
    {
        /// <summary>Used for completely removing / destroying objects from the scene</summary>
        public void Remove(object thing);
        public Grid2<T> Grid { get; }
    }
}