namespace BytingLib
{
    public interface IBoundingBoxByReference<T>
    {
        public BoundingBox GetBoundingBox(T reference);
    }
}
