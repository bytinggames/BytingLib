
namespace BytingLib
{
    public class Grid2List<T> : Grid2<T> where T : IBoundingRect
    {
        public Grid2List(float fieldSize)
            :base(fieldSize)
        {
        }

        public List<T> Entities { get; private set; } = new List<T>();

        public override void Add(T entity)
        {
            Entities.Add(entity);

            base.Add(entity);
        }
        public override bool Remove(T entity)
        {
            bool anyFail = base.Remove(entity);

            Entities.Remove(entity);

            return !anyFail;
        }
    }
}
