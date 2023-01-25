namespace BytingLib
{
    public class Grid3List<T> : Grid3<T> where T : IBoundingBox
    {
        public List<T> Entities { get; private set; } = new List<T>();

        public Grid3List(float fieldSize) : base(fieldSize)
        {
        }
        public Grid3List(Vector3 fieldSize) : base(fieldSize)
        {
        }

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

        public override IEnumerable<T> GetEntities() => Entities.AsEnumerable();
    }
}
