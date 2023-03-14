using System.Diagnostics.CodeAnalysis;

namespace BytingLib
{
    public class AnyCollisionResult
    {
        public float? Distance, DistanceReversed;

        public bool? GetCollisionFromDist() => Distance == null || DistanceReversed == null ? null : Math.Sign(Distance!.Value * DistanceReversed!.Value) == -1;

        [MemberNotNullWhen(true, nameof(Distance))]
        public bool IsDistanceBetween0And1()
        {
            if (!Distance.HasValue)
                return false;
            return Distance.Value >= 0f && Distance.Value <= 1f;
        }
    }
}
