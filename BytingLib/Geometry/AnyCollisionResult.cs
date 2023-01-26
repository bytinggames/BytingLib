namespace BytingLib.Geometry
{
    public class AnyCollisionResult
    {
        public float? Distance, DistanceReversed;

        public bool? GetCollisionFromDist() => Distance == null || DistanceReversed == null ? null : Math.Sign(Distance!.Value * DistanceReversed!.Value) == -1;


        public bool IsDistanceBetween0And1()
        {
            if (!Distance.HasValue)
                return false;
            return Distance.Value >= 0f && Distance.Value <= 1f;
        }
    }
}
