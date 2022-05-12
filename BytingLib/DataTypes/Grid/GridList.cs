
namespace BytingLib
{
    public class GridList<T> : Grid<T> where T : IBoundingRect
    {
        public GridList(float fieldSize)
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
