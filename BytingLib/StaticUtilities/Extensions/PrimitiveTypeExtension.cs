namespace BytingLib.StaticUtilities.Extensions
{
    public static class PrimitiveTypeExtension
    {
        public static int GetPrimitiveCount(this PrimitiveType primitiveType, int indexCount)
        {
            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    return indexCount / 3;
                case PrimitiveType.TriangleStrip:
                    return indexCount - 2;
                case PrimitiveType.LineList:
                    return indexCount / 2;
                case PrimitiveType.LineStrip:
                    return indexCount - 1;
                case PrimitiveType.PointList:
                    return indexCount;
                default:
                    throw new BytingException(primitiveType + " currently not supported.");
            }
        }
    }
}
