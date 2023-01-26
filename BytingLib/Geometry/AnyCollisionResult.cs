namespace BytingLib.Geometry
{
    public class AnyCollisionResult
    {
        public float? Distance, DistanceReversed;

        public bool? GetCollisionFromDist() => Distance == null || DistanceReversed == null ? null : Math.Sign(Distance!.Value * DistanceReversed!.Value) == -1;
    }
}
