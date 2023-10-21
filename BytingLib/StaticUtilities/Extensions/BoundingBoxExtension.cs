namespace BytingLib
{
    public static class BoundingBoxExtension
    {
        public static BoundingBox Expand(this BoundingBox box, Vector3 expand)
        {
            if (expand.X > 0)
            {
                box.Max.X += expand.X;
            }
            else
            {
                box.Min.X += expand.X;
            }

            if (expand.Y > 0)
            {
                box.Max.Y += expand.Y;
            }
            else
            {
                box.Min.Y += expand.Y;
            }

            if (expand.Z > 0)
            {
                box.Max.Z += expand.Z;
            }
            else
            {
                box.Min.Z += expand.Z;
            }

            return box;
        }

        public static float DistanceSquaredToVector(this BoundingBox box, Vector3 v)
        {
            Vector3 inside = MoveVectorInside(box, v);
            return Vector3.DistanceSquared(v, inside);
        }

        public static Vector3 MoveVectorInside(this BoundingBox box, Vector3 pos)
        {
            Vector3 nearestInBox;
            if (pos.X < box.Min.X)
            {
                nearestInBox.X = box.Min.X;
            }
            else if (pos.X > box.Max.X)
            {
                nearestInBox.X = box.Max.X;
            }
            else
            {
                nearestInBox.X = pos.X;
            }

            if (pos.Y < box.Min.Y)
            {
                nearestInBox.Y = box.Min.Y;
            }
            else if (pos.Y > box.Max.Y)
            {
                nearestInBox.Y = box.Max.Y;
            }
            else
            {
                nearestInBox.Y = pos.Y;
            }

            if (pos.Z < box.Min.Z)
            {
                nearestInBox.Z = box.Min.Z;
            }
            else if (pos.Z > box.Max.Z)
            {
                nearestInBox.Z = box.Max.Z;
            }
            else
            {
                nearestInBox.Z = pos.Z;
            }

            return nearestInBox;
        }

        public static Vector3 GetCenter(this BoundingBox box) => (box.Max + box.Min) / 2f;
    }
}
